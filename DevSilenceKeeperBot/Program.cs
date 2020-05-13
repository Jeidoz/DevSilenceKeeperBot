using DevSilenceKeeperBot.Data;
using StructureMap;
using System;

namespace DevSilenceKeeperBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Container.For<ConsoleDependencyRegistry>();
            if (args.Length == 1)
            {
                var bot = container.GetInstance<IDevSilenceKeeper>();
                bot.Run();
            }
            else
            {
                Console.WriteLine("Для запуска бота задайте параметр токена");
                Console.WriteLine("./DevSilenceKeeper 012345:abcdf");
                return;
            }
        }
    }
}