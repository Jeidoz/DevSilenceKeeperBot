using DevSilenceKeeperBot.Entities;
using LiteDB;

namespace DevSilenceKeeperBot.Data
{
    public sealed class DbContext : IDbContext
    {
        public DbContext(string dbFilename)
        {
            ILiteDatabase db = new LiteDatabase(dbFilename);
            Chats = db.GetCollection<Chat>("chats");
        }

        public DbContext(ILiteDatabase database)
        {
            Chats = database.GetCollection<Chat>("chats");
        }

        public ILiteCollection<Chat> Chats { get; }
    }
}