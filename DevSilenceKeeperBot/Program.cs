using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace DevSilenceKeeperBot
{
    internal static class Program
    {
        internal static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "DevSilenceKeeper.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                CreateHostBuilder().Build().Run();
            }
            catch (Exception e)
            {
                Log.Logger.Fatal($"{e.Message}\n{e.StackTrace}");
                throw;
            }
            Log.CloseAndFlush();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSystemd()
                .ConfigureServices(services =>
                {
                    services.AddLogging();

                    services.AddDbContext<BotDbContext>();

                    services.AddScoped<IChatService, ChatService>();
                    services.AddSingleton<IHostedService, DevSilenceKeeper>();
                })
                .ConfigureLogging(configLogging => configLogging.AddSerilog(dispose: true));
        }
    }
}