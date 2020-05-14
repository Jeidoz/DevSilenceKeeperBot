using DevSilenceKeeperBot.Entities;
using LiteDB;
using System.Diagnostics;

namespace DevSilenceKeeperBot.Data
{
    public sealed class DbContext : IDbContext
    {
        private readonly LiteDatabase _db;

        public ILiteCollection<Chat> Chats { get; }

        public DbContext()
        {
            // Database's filename = Current app name + .db
            _db = new LiteDatabase($"{Process.GetCurrentProcess().ProcessName}.db");
            Chats = _db.GetCollection<Chat>("chats");
        }
        public DbContext(LiteDatabase database)
        {
            _db = database;
            Chats = _db.GetCollection<Chat>("chats");
        }
    }
}