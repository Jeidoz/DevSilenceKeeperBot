using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Entities;
using DevSilenceKeeperBot.Services;
using LiteDB;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace DevSilenceKeeperBot.Tests
{
    public class ChatServiceTests
    {
        #region GetChatForbiddenWords
        [Fact]
        public void GetChatForbiddenWords_NonExistingChatId_ReturnsNull()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            var chatService = new ChatService(memoryDataContext);
            long nonExistsChatId = 1;

            var forbiddenWords = chatService.GetChatForbiddenWords(nonExistsChatId);

            Assert.Null(forbiddenWords);
        }

        [Fact]
        public void GetChatForbiddenWords_ExistingChatIdWithForbiddenWords_ReturnsWords()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            var chat = new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = new List<string> { "t.me/" }
            };
            memoryDataContext.Chats.Insert(chat);
            var chatService = new ChatService(memoryDataContext);

            var forbiddenWords = chatService.GetChatForbiddenWords(chat.Id);

            Assert.Equal(chat.ForbiddenWords, forbiddenWords);
        }

        [Fact]
        public void GetChatForbiddenWords_ExistingChatIdWithoutForbiddenWords_ReturnsNull()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            var chat = new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = null
            };
            memoryDataContext.Chats.Insert(chat);
            var chatService = new ChatService(memoryDataContext);

            var forbiddenWords = chatService.GetChatForbiddenWords(chat.Id);

            Assert.Null(forbiddenWords);
        }
        #endregion
    }
}