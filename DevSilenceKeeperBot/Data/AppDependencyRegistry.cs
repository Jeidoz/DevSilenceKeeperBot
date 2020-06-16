using System.Diagnostics;
using DevSilenceKeeperBot.Logging;
using StructureMap;

namespace DevSilenceKeeperBot.Data
{
    public sealed class ConsoleDependencyRegistry : Registry
    {
        public ConsoleDependencyRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.Exclude(type => type.Namespace != null && type.Namespace.Contains(value: "LiteDb"));
                scan.WithDefaultConventions();
            });

            // Database filename = Current app name + .db
            For<IDbContext>()
                .Singleton()
                .Use<DbContext>()
                .Ctor<string>(constructorArg: "dbFilename")
                .Is($"{Process.GetCurrentProcess().ProcessName}.db");

            For<ILogger>().Use<ConsoleLogger>();
        }
    }
}