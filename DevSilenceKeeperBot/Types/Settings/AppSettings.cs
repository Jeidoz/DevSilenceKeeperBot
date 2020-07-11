// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DevSilenceKeeperBot.Types.Settings
{
    public sealed class AppSettings
    {
        public string ConnectionString { get; set; }
        public string BotToken { get; set; }
        public string[] HelloWords { get; set; }
        public string[] GoogleWords { get; set; }
    }
}