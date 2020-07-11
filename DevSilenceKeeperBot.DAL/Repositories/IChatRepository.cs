using System.Collections.Generic;
using System.Threading.Tasks;
using DevSilenceKeeperBot.DAL.Entities;

namespace DevSilenceKeeperBot.DLL.Repositories
{
    public interface IChatRepository : IRepository<Chat>
    {
        Task<IEnumerable<string[]>> GetChatForbiddenWordsAsync();
        Task<IEnumerable<PromotedChatMember>> GetAllPromotedChatMembersAsync();
    }
}