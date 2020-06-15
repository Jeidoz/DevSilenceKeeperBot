using System;

namespace DevSilenceKeeperBot.Logging
{
    public sealed class ConsoleLogger : ILogger
    {
        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[INFO]: {text}");
        }

        public void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING]: {text}");
        }

        public void Error(string text, Exception ex = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR]: {text}");

            if (ex != null)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}