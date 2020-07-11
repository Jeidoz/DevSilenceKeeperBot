using System.Collections.Generic;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Data.Entities;
using Telegram.Bot.Types;
using Chat = DevSilenceKeeperBot.Data.Entities.Chat;

namespace DevSilenceKeeperBot.Services
{
    public interface IChatService
    {
        Task<List<string>> GetForbiddenWordsAsync(long chatId);

        Task<Chat> AddForbiddenWordAsync(long chatId, string word);

        Task<Chat> RemoveForbiddenWordAsync(long chatId, string word);

        Task<List<PromotedChatMember>> GetPromotedMembersAsync(long chatId);

        Task<Chat> AddPromotedMemberAsync(long chatId, User chatMember);

        Task RemovePromotedMemberAsync(long chatId, int memberId);
    }
}