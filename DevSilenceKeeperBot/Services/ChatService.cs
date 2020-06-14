using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Entities;
using DevSilenceKeeperBot.Exceptions;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Chat = DevSilenceKeeperBot.Entities.Chat;

namespace DevSilenceKeeperBot.Services
{
    public sealed class ChatService : IChatService
    {
        private readonly IDbContext _context;

        public ChatService(IDbContext dbContext)
        {
            _context = dbContext;
        }

        public IEnumerable<string> GetChatForbiddenWords(long chatId)
        {
            return _context.Chats.FindOne(chat => chat.ChatId == chatId)?.ForbiddenWords;
        }
        public Chat AddChatForbiddenWord(long chatId, string word)
        {
            var chat = _context.Chats.FindOne(chat => chat.ChatId == chatId);
            if (chat != null)
            {
                if(chat.ForbiddenWords.Contains(word))
                {
                    throw new AddingDublicateRecord("Данная строка-шаблон уже существует в банлисте", nameof(word));
                }
                chat.ForbiddenWords.Add(word);
                _context.Chats.Update(chat);
                return chat;
            }
            else
            {
                var newChat = new Chat
                {
                    ChatId = chatId,
                    ForbiddenWords = new List<string> { word },
                    PromotedMembers = new List<PromotedMember>()
                };
                newChat.Id = _context.Chats.Insert(newChat);

                return newChat;
            }
        }
        public bool RemoveChatForbiddenWord(long chatId, string word)
        {
            var chat = _context.Chats.FindOne(c => c.ChatId == chatId);
            if (chat != null)
            {
                if (!chat.ForbiddenWords.Contains(word))
                {
                    throw new RemovingNotExistingRecordException("Данная строка-шаблон отсуствует в банлисте", nameof(word));
                }
                chat.ForbiddenWords.Remove(word);
                return _context.Chats.Update(chat);
            }
            return false;
        }
        public IEnumerable<PromotedMember> GetPromotedMembers(long chatId)
        {
            return _context.Chats.FindOne(chat => chat.ChatId == chatId)?.PromotedMembers;
        }
        public Chat AddPromotedMember(long chatId, User chatMember)
        {
            var chat = _context.Chats.FindOne(chat => chat.ChatId == chatId);
            if (chat != null)
            {
                if(chat.PromotedMembers == null)
                {
                    chat.PromotedMembers = new List<PromotedMember>();
                }
                if (chat.PromotedMembers.Any(member => member.UserId == chatMember.Id))
                {
                    throw new AddingDublicateRecord("Данный пользователь уже имеет бонусные привылегии.", nameof(chatMember.Id));
                }

                var newPromotedUser = new PromotedMember
                {
                    UserId = chatMember.Id,
                    Username = chatMember.Username
                };
                chat.PromotedMembers.Add(newPromotedUser);
                _context.Chats.Update(chat);
                return chat;
            }
            else
            {
                var newPromotedUser = new PromotedMember
                {
                    UserId = chatMember.Id,
                    Username = chatMember.Username
                };
                var newChat = new Chat
                {
                    ChatId = chatId,
                    ForbiddenWords = new List<string>(),
                    PromotedMembers = new List<PromotedMember> { newPromotedUser }
                };
                newChat.Id = _context.Chats.Insert(newChat);

                return newChat;
            }
        }
        public bool RemovePromotedMember(long chatId, int userId)
        {
            var chat = _context.Chats.FindOne(c => c.ChatId == chatId);
            if (chat != null)
            {
                if (!chat.PromotedMembers.Any(member => member.UserId == userId))
                {
                    throw new RemovingNotExistingRecordException("Данный пользователь не имел бонусных привелегий.", nameof(userId));
                }
                var userForDelete = chat.PromotedMembers.FirstOrDefault(member => member.UserId == userId);
                chat.PromotedMembers.Remove(userForDelete);
                return _context.Chats.Update(chat);
            }
            return false;
        }
    }
}