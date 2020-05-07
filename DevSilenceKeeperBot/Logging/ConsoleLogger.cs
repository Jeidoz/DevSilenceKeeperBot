using System;

namespace DevSilenceKeeperBot.Logging
{
    public static class ConsoleLogger
    {
        public static void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[INFO]: {text}");
        }

        public static void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING]: {text}");
        }

        public static void Error(string text, Exception ex = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR]: {text}");

            if(ex != null)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}