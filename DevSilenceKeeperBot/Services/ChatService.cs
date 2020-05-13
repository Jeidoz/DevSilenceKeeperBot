using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Entities;
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
        public bool AddChatForbiddenWord(long chatId, string word)
        {
            var chat = _context.Chats.FindOne(chat => chat.ChatId == chatId);
            if (chat != null)
            {
                chat.ForbiddenWords.Add(word);
                return _context.Chats.Update(chat);
            }
            else
            {
                _context.Chats.Insert(new Chat
                {
                    ChatId = chatId,
                    ForbiddenWords = new List<string> { word }
                });
                return true;
            }
        }
        public bool RemoveChatForbiddenWord(long chatId, string word)
        {
            var chat = _context.Chats.FindOne(c => c.ChatId == chatId);
            if (chat != null)
            {
                chat.ForbiddenWords.Remove(word);
                return _context.Chats.Update(chat);
            }
            return false;
        }
    }
}