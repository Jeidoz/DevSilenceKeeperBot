using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Entities;
using DevSilenceKeeperBot.Exceptions;
using System.Collections.Generic;

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
                    ForbiddenWords = new List<string> { word }
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
    }
}