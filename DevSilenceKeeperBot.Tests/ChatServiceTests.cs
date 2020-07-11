using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Data.Entities;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace DevSilenceKeeperBot.Tests
{
    public class ChatServiceTests
    {
        private const long ChatId = 1;
        private const int UserId = 1;
        private static DbContextOptions<BotDbContext> CreateNewContextOptions()
        {
            var builder = new DbContextOptionsBuilder<BotDbContext>();
            builder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            return builder.Options;
        }

        #region Context seed methods

        private BotDbContext GetContextWithFilledChat()
        {
            var context = new BotDbContext(CreateNewContextOptions());
            AddOnePromotedChatMember(context);
            AddOneChatWithTwoForbiddenWordsAndOnePromotedMember(context);
            return context;
        }
        private void AddOnePromotedChatMember(BotDbContext context)
        {
            var promotedChatMember = new PromotedChatMember
            {
                Id = 1,
                Username = "testUsername",
                FullName = "User for tests",
                UserId = UserId
            };
            context.PromotedMembers.Add(promotedChatMember);
            context.SaveChanges();
        }
        private void AddOneChatWithTwoForbiddenWordsAndOnePromotedMember(BotDbContext context)
        {
            var chat = new Chat(ChatId)
            {
                ChatId = ChatId,
                ForbiddenWords = new List<ForbiddenChatWord>
                {
                    new ForbiddenChatWord {Id = 1, Word = "abcd"},
                    new ForbiddenChatWord {Id = 2, Word = "1234"},
                }
            };
            context.Chats.Add(chat);
            context.SaveChanges();
            
            var chatPromotedMember = new ChatToPromotedMember
            {
                Chat = chat,
                PromotedChatMember = context.PromotedMembers.Single(pm => pm.UserId == 1)
            };
            chat.PromotedMembers.Add(chatPromotedMember);
            context.SaveChanges();
        }

        private BotDbContext GetContextWithEmptyChat()
        {
            var context = new BotDbContext(CreateNewContextOptions());
            context.Chats.Add(new Chat(ChatId));
            context.SaveChanges();
            return context;
        }
        
        #endregion

        #region GetForbiddenWordsAsync

        [Fact]
        public async Task GetForbiddenWordsAsync_NonExistingChat_ReturnsEmptyList()
        {
            // Arrange
            
            var service = new ChatService(new BotDbContext(CreateNewContextOptions()));
            
            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);
            
            // Assert
            Assert.Empty(words);
        }

        [Fact]
        public async Task GetForbiddenWordsAsync_ChatWithoutForbiddenWords_ReturnsEmptyList()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);
            
            // Assert
            Assert.True(context.Chats.Any(ch => ch.ChatId == ChatId));
            Assert.Empty(words);
        }

        [Fact]
        public async Task GetPromotedMembersAsync_ChatWithForbiddenWords_ReturnsListWithForbiddenWords()
        {
            // Arrange
            
            var context = GetContextWithFilledChat();
            var service = new ChatService(context);
            
            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);
            
            // Assert
            Assert.Equal(2, words.Count);
            Assert.Collection(words, 
                item => Assert.Equal("abcd", item),
                item => Assert.Equal("1234", item));
        }

        #endregion

        #region AddForbiddenWordAsync

        [Fact]
        public async Task AddForbiddenWordAsync_Null_ThrowsArgumentNullException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task AddForbiddenWords() => await service.AddForbiddenWordAsync(ChatId, null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(AddForbiddenWords);
        }
        
        [Fact]
        public async Task AddForbiddenWordAsync_EmptyString_ThrowsArgumentNullException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task AddForbiddenWords() => await service.AddForbiddenWordAsync(ChatId, string.Empty);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(AddForbiddenWords);
        }
        
        [Fact]
        public async Task AddForbiddenWordAsync_UnderMinLengthWord_ThrowsArgumentException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task AddForbiddenWords() => await service.AddForbiddenWordAsync(ChatId, "bob");

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(AddForbiddenWords);
        }
        
        [Fact]
        public async Task AddForbiddenWordAsync_CorrectWord_ReturnsChatWithAddedForbiddenWord()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            var chatWithForbiddenWord = await service.AddForbiddenWordAsync(ChatId, "java");

            // Assert
            Assert.NotEmpty(chatWithForbiddenWord.ForbiddenWords);
            Assert.Collection(chatWithForbiddenWord.ForbiddenWords, 
                item => Assert.Equal("java", item.Word));
        }
        
        [Fact]
        public async Task AddForbiddenWordAsync_DuplicateWord_ThrowsAddingDuplicateRecordException()
        {
            // Arrange
            
            var context = GetContextWithFilledChat();
            var service = new ChatService(context);
            
            // Act
            async Task AddChatWithForbiddenWord() => await service.AddForbiddenWordAsync(ChatId, "abcd");

            // Assert
            await Assert.ThrowsAsync<AddingDuplicateRecord>(AddChatWithForbiddenWord);
        }

        [Fact]
        public async Task AddForbiddenWordAsync_CorrectWordAndNonExistChat_ReturnsChatWithForbiddenWord()
        {
            // Arrange
            
            var context = new BotDbContext(CreateNewContextOptions());
            var service = new ChatService(context);
            
            // Act
            var chatWithForbiddenWord = await service.AddForbiddenWordAsync(ChatId, "java");
            
            // Assert
            Assert.True(context.Chats.Any(ch => ch.ChatId == ChatId));
            Assert.Collection(chatWithForbiddenWord.ForbiddenWords,
                item => Assert.Equal("java", item.Word));
        }

        #endregion

        #region RemoveForbiddenWordAsync

        [Fact]
        private async Task RemoveForbiddenWordAsync_Null_ThrowsNullArgumentException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task RemoveForbiddenWords() => await service.RemoveForbiddenWordAsync(ChatId, null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(RemoveForbiddenWords);
        }

        [Fact]
        private async Task RemoveForbiddenWordAsync_EmptyString_ThrowsNullArgumentException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task RemoveForbiddenWords() => await service.RemoveForbiddenWordAsync(ChatId, string.Empty);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(RemoveForbiddenWords);
        }
        
        [Fact]
        private async Task RemoveForbiddenWordAsync_UnderMinLengthWord_ThrowsArgumentException()
        {
            // Arrange
            
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            async Task RemoveForbiddenWords() => await service.RemoveForbiddenWordAsync(ChatId, "bob");

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(RemoveForbiddenWords);
        }
        
        [Fact]
        public async Task RemoveForbiddenWordAsync_NotBannedWord_ThrowsRemovingNotExistingRecordException()
        {
            // Arrange
            
            var context = GetContextWithFilledChat();
            var service = new ChatService(context);
            
            // Act
            async Task RemoveChatWithForbiddenWord() => await service.RemoveForbiddenWordAsync(ChatId, "java");

            // Assert
            await Assert.ThrowsAsync<RemovingNotExistingRecordException>(RemoveChatWithForbiddenWord);
        }
        
        [Fact]
        private async Task RemoveForbiddenWordAsync_CorrectWord_ReturnsChatWithRemovedForbiddenWord()
        {
            // Arrange
            
            var context = GetContextWithFilledChat();
            var service = new ChatService(context);
            
            // Act
            var chatWithForbiddenWord = await service.RemoveForbiddenWordAsync(ChatId, "abcd");

            // Assert
            Assert.True(context.Chats.Any(ch => ch.ChatId == ChatId));
            Assert.Equal(1, chatWithForbiddenWord.ForbiddenWords.Count);
            Assert.Collection(chatWithForbiddenWord.ForbiddenWords,
                item => Assert.Equal("1234", item.Word));
        }


        #endregion

        #region GetChatPromotedMembersAsync

        [Fact]
        public async Task GetPromotedMembersAsync_NonExistingChat_ReturnsEmptyList()
        {
            // Arrange
            var service = new ChatService(new BotDbContext(CreateNewContextOptions()));
            
            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);
            
            // Assert
            Assert.Empty(promotedMembers);
        }

        [Fact]
        public async Task GetPromotedMembersAsync_ChatWithoutForbiddenWords_ReturnsEmptyList()
        {
            // Arrange
            var context = GetContextWithEmptyChat();
            var service = new ChatService(context);
            
            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);
            
            // Assert
            Assert.True(context.Chats.Any(ch => ch.ChatId == ChatId));
            Assert.Empty(promotedMembers);
        }

        [Fact]
        public async Task GetForbiddenWordsAsync_ChatWithPromotedMembers_ReturnsListWithPromotedMembers()
        {
            // Arrange
            
            var context = GetContextWithFilledChat();
            var service = new ChatService(context);
            
            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);
            
            // Assert
            Assert.Single(promotedMembers);
            Assert.Equal(UserId, promotedMembers[0].UserId);
            Assert.Equal(ChatId, promotedMembers[0].Chats[0].ChatId);
        }

        #endregion
    }
}