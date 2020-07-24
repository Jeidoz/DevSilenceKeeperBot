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
        private readonly IBotDbContextFactory _contextFactory;

        public ChatService(IBotDbContextFactory factory)
        {
            _contextFactory = factory;
        }

        private async Task<Chat> GetChatById(long chatId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var chat = await context.Chats.FirstOrDefaultAsync(ch => ch.ChatId == chatId);
            if (chat != null)
            {
                return chat;
            }

            chat = new Chat(chatId);
            context.Chats.Add(chat);
            await context.SaveChangesAsync();
            return chat;
        }

        private void ValidateForbiddenWord(in string word)
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

        public async Task<List<string>> GetForbiddenWordsAsync(long chatId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.ForbiddenChatWords
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
            await using var context = _contextFactory.CreateDbContext();
            context.Entry(await GetChatById(chatId)).CurrentValues.SetValues(requestedChat);
            await context.SaveChangesAsync();
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

            await using var context = _contextFactory.CreateDbContext();
            var wordToRemove = await context.ForbiddenChatWords
                .FirstAsync(w => w.Chat.ChatId == chatId && w.Word == word);
            context.ForbiddenChatWords.Remove(wordToRemove);
            await context.SaveChangesAsync();
            var chat = await context.Chats.SingleAsync(ch => ch.ChatId == chatId);
            return chat;
        }

        public async Task<List<PromotedChatMember>> GetPromotedMembersAsync(long chatId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.ChatToPromotedMembers
                .Include(c2pm => c2pm.Chat)
                .Include(c2pm => c2pm.PromotedChatMember)
                .ThenInclude(pm => pm.Chats)
                .Where(c2pm => c2pm.Chat.ChatId == chatId)
                .Select(c2pm => c2pm.PromotedChatMember)
                .ToListAsync();
        }

        public async Task<Chat> AddPromotedMemberAsync(long chatId, User chatMember)
        {
            await using var context = _contextFactory.CreateDbContext();
            var requestedChat = await context.Chats
                .Include(ch => ch.PromotedMembers)
                .SingleAsync(ch => ch.ChatId == chatId);
            var chatMemberToUpdate = await context.PromotedMembers
                                         .FirstOrDefaultAsync(pm => pm.UserId == chatMember.Id)
                                     ?? await AddNewPromotedChatMember(chatMember);

            requestedChat.PromotedMembers.Add(new ChatToPromotedMember
            {
                Chat = requestedChat,
                PromotedChatMember = chatMemberToUpdate
            });
            await context.SaveChangesAsync();

            return requestedChat;
        }

        private async Task<PromotedChatMember> AddNewPromotedChatMember(User user)
        {
            await using var context = _contextFactory.CreateDbContext();
            var promotedMember = new PromotedChatMember(user);
            context.PromotedMembers.Add(promotedMember);
            await context.SaveChangesAsync();
            return promotedMember;
        }

        public async Task RemovePromotedMemberAsync(long chatId, int memberId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var memberToRemove = await context.ChatToPromotedMembers
                .Include(c2pm => c2pm.PromotedChatMember)
                .Include(c2pm => c2pm.Chat)
                .SingleAsync(c2pm => c2pm.Chat.Id == chatId && c2pm.PromotedChatMember.UserId == memberId);
            context.ChatToPromotedMembers.Remove(memberToRemove);
            await context.SaveChangesAsync();
        }
    }
}