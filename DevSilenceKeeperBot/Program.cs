using System;
using System.IO;
using DevSilenceKeeperBot.Data;
using DevSilenceKeeperBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.File;

namespace DevSilenceKeeperBot
{
    internal static class Program
    {
        internal static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs","DevSilenceKeeper.txt"), rollingInterval: RollingInterval.Day)
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

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();

                    services.AddDbContext<BotDbContext>(options =>
                        options.UseMySql(Configuration.GetConnectionString("MySql")));
                    services.AddScoped<IChatService, ChatService>();
                    services.AddSingleton<IHostedService, DevSilenceKeeper>();
                })
                .ConfigureLogging(configLogging =>
                {
                    configLogging.AddSerilog();
                })
                .UseConsoleLifetime();
    }
}