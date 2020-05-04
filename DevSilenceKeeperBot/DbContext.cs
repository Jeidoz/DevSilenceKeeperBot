using DevSilenceKeeperBot.Entities;
using LiteDB;
using System.Collections.Generic;

namespace DevSilenceKeeperBot
{
    public sealed class DbContext
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<Chat> _chats;

        public DbContext(string dbFilename = "db.db")
        {
            _db = new LiteDatabase(dbFilename);
            _chats = _db.GetCollection<Chat>("chats");
        }

        public IEnumerable<string> GetChatForbiddenWords(long chatId)
        {
            return _chats.FindOne(chat => chat.ChatId == chatId)?.ForbiddenWords;
        }
        public bool AddChatForbiddenWord(long chatId, string word)
        {
            Chat chat = _chats.FindOne(chat => chat.ChatId == chatId);
            if(chat != null)
            {
                chat.ForbiddenWords.Add(word);
                return _chats.Update(chat);
            }
            else
            {
                _chats.Insert(new Chat
                {
                    ChatId = chatId,
                    ForbiddenWords = new List<string> { word }
                });
                return true;
            }
        }
        public bool RemoveChatForbiddenWord(long chatId, string word)
        {
            Chat chat = _chats.FindOne(c => c.ChatId == chatId);
            if (chat != null)
            {
                chat.ForbiddenWords.Remove(word);
                return _chats.Update(chat);
            }
            return false;
        }
    }
}