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
                scan.WithDefaultConventions();
            });

            For<IDbContext>().Singleton().Use<DbContext>();
        }
    }
}