using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Data.Entities;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;
using DevSilenceKeeperBot.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Chat = DevSilenceKeeperBot.Data.Entities.Chat;

namespace DevSilenceKeeperBot.Services
{
    public sealed class ChatService : IChatService
    {
        private const int MinForbiddenWordLength = 4;
        private readonly BotDbContext _context;

        public ChatService(BotDbContext context)
        {
            _context = context;
        }

        private async Task<Chat> GetChatById(long chatId)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(ch => ch.ChatId == chatId);
            if (chat != null)
            {
                return chat;
            }

            chat = new Chat(chatId);
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        private void ValidateForbiddenWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentNullException(nameof(word), "Forbidden word can't be null or empty.");
            }

            if (word.Length < MinForbiddenWordLength)
            {
                throw new ArgumentException("Forbidden word should be at least 3 characters long.", nameof(word));
            }
        }

        public Task<List<string>> GetForbiddenWordsAsync(long chatId)
        {
            return _context.ForbiddenChatWords
                .Where(word => word.Chat.ChatId == chatId)
                .Select(word => word.Word)
                .ToListAsync();
        }

        public async Task<Chat> AddForbiddenWordAsync(long chatId, string word)
        {
            ValidateForbiddenWord(word);
            var forbiddenChatWords = await GetForbiddenWordsAsync(chatId);
            if (forbiddenChatWords.Contains(word))
            {
                throw new AddingDuplicateRecord("Such forbidden word already added in the chat.",
                    nameof(word));
            }

            Chat requestedChat = await GetChatById(chatId);
            var newForbiddenWord = new ForbiddenChatWord
            {
                Chat = requestedChat,
                Word = word
            };
            requestedChat.ForbiddenWords.Add(newForbiddenWord);
            _context.Chats.Update(requestedChat);
            await _context.SaveChangesAsync();
            return requestedChat;
        }

        public async Task<Chat> RemoveForbiddenWordAsync(long chatId, string word)
        {
            ValidateForbiddenWord(word);
            var forbiddenChatWords = await GetForbiddenWordsAsync(chatId);
            if (!forbiddenChatWords.Contains(word))
            {
                throw new RemovingNotExistingRecordException("Such forbidden word wasn't banned in the chat.",
                    nameof(word));
            }

            var wordToRemove = await _context.ForbiddenChatWords
                .FirstAsync(w => w.Chat.ChatId == chatId && w.Word == word);
            _context.ForbiddenChatWords.Remove(wordToRemove);
            await _context.SaveChangesAsync();
            return await _context.Chats.SingleAsync(ch => ch.ChatId == chatId);

        }

        public Task<List<PromotedChatMember>> GetPromotedMembersAsync(long chatId)
        {
            return _context.ChatToPromotedMembers
                .Include(c2pm => c2pm.Chat)
                .Include(c2pm => c2pm.PromotedChatMember)
                    .ThenInclude(pm => pm.Chats)
                .Where(c2pm => c2pm.Chat.ChatId == chatId)
                .Select(c2pm => c2pm.PromotedChatMember)
                .ToListAsync();
        }

        public async Task<Chat> AddPromotedMemberAsync(long chatId, User chatMember)
        {
            var requestedChat = await _context.Chats
                .Include(ch => ch.PromotedMembers)
                .SingleAsync(ch => ch.ChatId == chatId);
            var chatMemberToUpdate = await _context.PromotedMembers
                .FirstOrDefaultAsync(pm => pm.UserId == chatMember.Id)
                                     ?? await AddNewPromotedChatMember(chatMember);

            requestedChat.PromotedMembers.Add(new ChatToPromotedMember
            {
                Chat = requestedChat,
                PromotedChatMember = chatMemberToUpdate
            });
            await _context.SaveChangesAsync();

            return requestedChat;
        }

        private async Task<PromotedChatMember> AddNewPromotedChatMember(User user)
        {
            var promotedMember = new PromotedChatMember(user);
            _context.PromotedMembers.Add(promotedMember);
            await _context.SaveChangesAsync();
            return promotedMember;
        }

        public async Task RemovePromotedMemberAsync(long chatId, int memberId)
        {
            var memberToRemove = await _context.ChatToPromotedMembers
                .Include(c2pm => c2pm.PromotedChatMember)
                .Include(c2pm => c2pm.Chat)
                .SingleAsync(c2pm => c2pm.Chat.Id == chatId && c2pm.PromotedChatMember.UserId == memberId);
            _context.ChatToPromotedMembers.Remove(memberToRemove);
            await _context.SaveChangesAsync();
        }
    }
}