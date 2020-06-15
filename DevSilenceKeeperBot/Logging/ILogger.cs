using System;

namespace DevSilenceKeeperBot.Logging
{
    public interface ILogger
    {
        void Info(string text);

        void Warning(string text);

        void Error(string text, Exception ex = null);
    }
}