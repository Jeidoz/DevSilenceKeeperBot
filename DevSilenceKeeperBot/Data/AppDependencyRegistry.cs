using LiteDB;
using StructureMap;
using System.Diagnostics;

namespace DevSilenceKeeperBot.Data
{
    public sealed class ConsoleDependencyRegistry : Registry
    {
        public ConsoleDependencyRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.Exclude(type => type.Namespace.Contains("LiteDb"));
                scan.WithDefaultConventions();
            });

            // Database's filename = Current app name + .db
            For<IDbContext>()
                .Singleton()
                .Use<DbContext>()
                .Ctor<string>("dbFilename")
                    .Is($"{Process.GetCurrentProcess().ProcessName}.db");
        }
    }
}