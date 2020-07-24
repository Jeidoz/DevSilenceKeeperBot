
// ReSharper disable StringLiteralTypo

using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Data.Entities;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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

        private static BotDbContext GetContextWithFilledChat()
        {
            var context = new BotDbContext(CreateNewContextOptions());
            context.Database.EnsureDeleted();
            AddOnePromotedChatMember(context);
            AddOneChatWithTwoForbiddenWordsAndOnePromotedMember(context);
            return context;
        }

        private static void AddOnePromotedChatMember(BotDbContext context)
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

        private static void AddOneChatWithTwoForbiddenWordsAndOnePromotedMember(BotDbContext context)
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

        private static BotDbContext GetContextWithEmptyChat()
        {
            var context = new BotDbContext(CreateNewContextOptions());
            context.Database.EnsureDeleted();
            context.Chats.Add(new Chat(ChatId));
            context.SaveChanges();
            return context;
        }

        #endregion Context seed methods

        #region Fakes of IBotDbContextFactory

        private class EmptyDbContextFactory : IBotDbContextFactory
        {
            public BotDbContext CreateDbContext()
            {
                var context = new BotDbContext(CreateNewContextOptions());
                context.Database.EnsureDeleted();
                return context;
            }
        }

        private class EmptyChatDbContextFactory : IBotDbContextFactory
        {
            public BotDbContext CreateDbContext()
            {
                return GetContextWithEmptyChat();
            }
        }

        private class FilledChatDbContextFactory : IBotDbContextFactory
        {
            public BotDbContext CreateDbContext()
            {
                return GetContextWithFilledChat();
            }
        }

        #endregion

        #region GetForbiddenWordsAsync

        [Fact]
        public async Task GetForbiddenWordsAsync_NonExistingChat_ReturnsEmptyList()
        {
            // Arrange
            var service = new ChatService(new EmptyDbContextFactory());

            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);

            // Assert
            Assert.Empty(words);
        }

        [Fact]
        public async Task GetForbiddenWordsAsync_ChatWithoutForbiddenWords_ReturnsEmptyList()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);

            // Assert
            Assert.Empty(words);
        }

        [Fact]
        public async Task GetPromotedMembersAsync_ChatWithForbiddenWords_ReturnsListWithForbiddenWords()
        {
            // Arrange
            var service = new ChatService(new FilledChatDbContextFactory());

            // Act
            var words = await service.GetForbiddenWordsAsync(ChatId);

            // Assert
            Assert.Equal(2, words.Count);
            Assert.Collection(words,
                item => Assert.Equal("abcd", item),
                item => Assert.Equal("1234", item));
        }

        #endregion GetForbiddenWordsAsync

        #region AddForbiddenWordAsync

        [Fact]
        public async Task AddForbiddenWordAsync_Null_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.AddForbiddenWordAsync(ChatId, null)
            );
        }

        [Fact]
        public async Task AddForbiddenWordAsync_EmptyString_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.AddForbiddenWordAsync(ChatId, string.Empty)
            );
        }

        [Fact]
        public async Task AddForbiddenWordAsync_UnderMinLengthWord_ThrowsArgumentException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.AddForbiddenWordAsync(ChatId, "bob")
            );
        }

        [Fact]
        public async Task AddForbiddenWordAsync_CorrectWord_ReturnsChatWithAddedForbiddenWord()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

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
            var service = new ChatService(new FilledChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<AddingDuplicateRecord>(async () =>
                await service.AddForbiddenWordAsync(ChatId, "abcd")
            );
        }

        [Fact]
        public async Task AddForbiddenWordAsync_CorrectWordAndNonExistChat_ReturnsChatWithForbiddenWord()
        {
            // Arrange
            var service = new ChatService(new EmptyDbContextFactory());

            // Act
            var chatWithForbiddenWord = await service.AddForbiddenWordAsync(ChatId, "java");

            // Assert
            Assert.Collection(chatWithForbiddenWord.ForbiddenWords,
                item => Assert.Equal("java", item.Word));
        }

        #endregion AddForbiddenWordAsync

        #region RemoveForbiddenWordAsync

        [Fact]
        private async Task RemoveForbiddenWordAsync_Null_ThrowsNullArgumentException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.RemoveForbiddenWordAsync(ChatId, null)
            );
        }

        [Fact]
        private async Task RemoveForbiddenWordAsync_EmptyString_ThrowsNullArgumentException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.RemoveForbiddenWordAsync(ChatId, string.Empty)
            );
        }

        [Fact]
        private async Task RemoveForbiddenWordAsync_UnderMinLengthWord_ThrowsArgumentException()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RemoveForbiddenWordAsync(ChatId, "bob")
            );
        }

        [Fact]
        public async Task RemoveForbiddenWordAsync_NotBannedWord_ThrowsRemovingNotExistingRecordException()
        {
            // Arrange
            var service = new ChatService(new FilledChatDbContextFactory());

            // Act & Assert
            await Assert.ThrowsAsync<RemovingNotExistingRecordException>(async () =>
                await service.RemoveForbiddenWordAsync(ChatId, "java")
            );
        }

        [Fact]
        private async Task RemoveForbiddenWordAsync_CorrectWord_ReturnsChatWithRemovedForbiddenWord()
        {
            // Arrange
            var service = new ChatService(new FilledChatDbContextFactory());

            // Act
            var chatWithForbiddenWord = await service.RemoveForbiddenWordAsync(ChatId, "abcd");

            // Assert
            Assert.Equal(1, chatWithForbiddenWord.ForbiddenWords.Count);
            Assert.Collection(chatWithForbiddenWord.ForbiddenWords,
                item => Assert.Equal("1234", item.Word));
        }

        #endregion RemoveForbiddenWordAsync

        #region GetChatPromotedMembersAsync

        [Fact]
        public async Task GetPromotedMembersAsync_NonExistingChat_ReturnsEmptyList()
        {
            // Arrange
            var service = new ChatService(new EmptyDbContextFactory());

            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);

            // Assert
            Assert.Empty(promotedMembers);
        }

        [Fact]
        public async Task GetPromotedMembersAsync_ChatWithoutForbiddenWords_ReturnsEmptyList()
        {
            // Arrange
            var service = new ChatService(new EmptyChatDbContextFactory());

            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);

            // Assert
            Assert.Empty(promotedMembers);
        }

        [Fact]
        public async Task GetForbiddenWordsAsync_ChatWithPromotedMembers_ReturnsListWithPromotedMembers()
        {
            // Arrange
            var service = new ChatService(new FilledChatDbContextFactory());

            // Act
            var promotedMembers = await service.GetPromotedMembersAsync(ChatId);

            // Assert
            Assert.Single(promotedMembers);
            Assert.Equal(UserId, promotedMembers[0].UserId);
            Assert.Equal(ChatId, promotedMembers[0].Chats[0].ChatId);
        }

        #endregion GetChatPromotedMembersAsync
    }
}