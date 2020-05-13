using DevSilenceKeeperBot.Entities;
using LiteDB;

namespace DevSilenceKeeperBot.Data
{
    public interface IDbContext
    {
        public ILiteCollection<Chat> Chats { get; }
    }
}