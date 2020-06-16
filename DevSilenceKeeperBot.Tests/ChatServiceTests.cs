using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Entities;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        #region AddChatForbiddenWord
        [Fact]
        public void AddChatForbiddenWord_FirstWordInChat_ReturnsChatWithOneForbiddenWords()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            var chatService = new ChatService(memoryDataContext);

            var result = chatService.AddChatForbiddenWord(1, "t.me/");

            Assert.True(result.ForbiddenWords.Count == 1);
            Assert.Equal("t.me/", result.ForbiddenWords.First());
        }

        [Fact]
        public void AddChatForbiddenWord_AddWordInExistingChat_ReturnsChatWithTwoForbiddenWords()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            memoryDataContext.Chats.Insert(new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = new List<string> { "t.me/" }
            });
            var chatService = new ChatService(memoryDataContext);

            var result = chatService.AddChatForbiddenWord(1, "bit.ly/");

            Assert.True(result.ForbiddenWords.Count == 2);
            Assert.Equal("bit.ly/", result.ForbiddenWords.Last());
        }

        [Fact]
        public void AddChatForbiddenWord_AddExistingWord_ReturnsChatWithoutChanges()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            memoryDataContext.Chats.Insert(new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = new List<string> { "t.me/" }
            });
            var chatService = new ChatService(memoryDataContext);

            Action testingAction = () => chatService.AddChatForbiddenWord(1, "t.me/");

            Assert.Throws<AddingDuplicateRecord>(testingAction);
        }
        #endregion

        #region RemoveChatForbiddenWord
        [Fact]
        public void RemoveChatForbiddenWord_WordExistInChat_ReturnsTrue()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            memoryDataContext.Chats.Insert(new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = new List<string> { "t.me/" }
            });
            var chatService = new ChatService(memoryDataContext);

            bool isRemoved = chatService.RemoveChatForbiddenWord(1, "t.me/");

            Assert.True(isRemoved);
        }

        [Fact]
        public void RemoveChatForbiddenWord_WordNotExistInChat_ThrowsRemovingNotExistingRecordException()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            memoryDataContext.Chats.Insert(new Chat
            {
                Id = 1,
                ChatId = 1,
                ForbiddenWords = new List<string> { "t.me/" }
            });
            var chatService = new ChatService(memoryDataContext);

            Action testingAction = () => chatService.RemoveChatForbiddenWord(1, "bit.ly/");

            Assert.Throws<RemovingNotExistingRecordException>(testingAction);
        }

        [Fact]
        public void RemoveChatForbiddenWord_ChatNotExist_ReturnsFalse()
        {
            var memoryDb = new LiteDatabase(new MemoryStream());
            var memoryDataContext = new DbContext(memoryDb);
            var chatService = new ChatService(memoryDataContext);

            bool result = chatService.RemoveChatForbiddenWord(1, "t.me/");

            Assert.False(result);
        }
        #endregion
    }
}