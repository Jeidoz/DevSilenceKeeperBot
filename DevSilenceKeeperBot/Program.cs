using DevSilenceKeeperBot.Data;
using StructureMap;

namespace DevSilenceKeeperBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Container.For<ConsoleDependencyRegistry>();
            var bot = container.GetInstance<IDevSilenceKeeper>();
            bot.Run();
        }
    }
}