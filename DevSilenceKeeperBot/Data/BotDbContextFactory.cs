using Microsoft.EntityFrameworkCore;

namespace DevSilenceKeeperBot.Data
{
    public sealed class BotDbContextFactory : IBotDbContextFactory
    {
        private readonly DbContextOptions<BotDbContext> _options;
        public BotDbContextFactory(DbContextOptions<BotDbContext> options)
        {
            _options = options;
        }

        public BotDbContext CreateDbContext()
        {
            return new BotDbContext(_options);
        }
    }
}