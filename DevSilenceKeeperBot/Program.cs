using DevSilenceKeeperBot.Data;
using StructureMap;
using System;

namespace DevSilenceKeeperBot
{
    class Program
    {
        static IDevSilenceKeeper _bot;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            var container = Container.For<ConsoleDependencyRegistry>();
            _bot = container.GetInstance<IDevSilenceKeeper>();
            _bot.Run();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _bot.Cancel();
        }
    }
}