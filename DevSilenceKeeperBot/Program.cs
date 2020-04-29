using System;
using System.IO;

namespace DevSilenceKeeperBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DevSilenceKeeper bot;
                if (args.Length == 1)
                {
                    bot = new DevSilenceKeeper(args[0]);
                }
                else if (args.Length == 2)
                {
                    bot = new DevSilenceKeeper(args[0], args[1]);
                }
                else
                {
                    Console.WriteLine("Для запуска бота задайте параметр токена (и опционально путь к файлу с запрещенных слов)");
                    Console.WriteLine("./DevSilenceKeeper 012345:abcdf [words.txt]");
                    return;
                }

                bot.StartPolling();
                Console.WriteLine("Введите \"stop\" что бы остановить бота.");
                do
                {
                    if (Console.ReadLine() == "stop")
                        break;
                }
                while (true);
                bot.StopPolling();
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("Не удалось найти файл из запрещенными словами.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Не удалось найти файл из запрещенными словами.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Во время роботы случилась ошибка:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return;
            }
        }
    }
}