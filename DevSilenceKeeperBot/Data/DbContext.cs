using DevSilenceKeeperBot.Entities;
using LiteDB;

namespace DevSilenceKeeperBot.Data
{
    public sealed class DbContext : IDbContext
    {
        private readonly ILiteDatabase _db;

        public ILiteCollection<Chat> Chats { get; }

        public DbContext(string dbFilename)
        {
            _db = new LiteDatabase(dbFilename);
            Chats = _db.GetCollection<Chat>("chats");
        }

        public DbContext(ILiteDatabase database)
        {
            _db = database;
            Chats = _db.GetCollection<Chat>("chats");
        }
    }
}