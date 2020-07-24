namespace DevSilenceKeeperBot.Data
{
    public interface IBotDbContextFactory
    {
        BotDbContext CreateDbContext();
    }
}