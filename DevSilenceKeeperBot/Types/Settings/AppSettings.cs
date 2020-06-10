using System.Collections.Generic;

namespace DevSilenceKeeperBot.Types.Settings
{
    public sealed class AppSettings
    {
        public string BotToken { get; set; }
        public string[] HelloWords { get; set; }
        public string[] GoogleWords { get; set; }
    }
}