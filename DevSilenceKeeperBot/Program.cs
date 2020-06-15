using DevSilenceKeeperBot.Data;
using StructureMap;
using System;

namespace DevSilenceKeeperBot
{
    internal static class Program
    {
        private static IDevSilenceKeeper _bot;

        private static void Main()
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