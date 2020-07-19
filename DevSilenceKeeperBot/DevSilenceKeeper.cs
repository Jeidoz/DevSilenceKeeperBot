using DevSilenceKeeperBot.Commands;
using DevSilenceKeeperBot.Commands.Callback;
using DevSilenceKeeperBot.Services;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace DevSilenceKeeperBot
{
    // ReSharper disable once UnusedType.Global
    public sealed class DevSilenceKeeper : IHostedService
    {
        public static ITelegramBotClient BotClient;

        private readonly IChatService _chatService;
        private readonly List<Command> _commands;
        private readonly List<CallbackCommand> _callbackCommands;

        public DevSilenceKeeper(IChatService chatService)
        {
            InitializeBotClientInstance(Program.Configuration["BotToken"]);
            BotClient.OnMessage += OnMessage;
            BotClient.OnCallbackQuery += OnCallbackQuery;

            _chatService = chatService;

            InitializeListOfBotCommands(out _commands);
            InitializeListOfBotCallbackCommands(out _callbackCommands);
        }

        private static void InitializeBotClientInstance(string botToken)
        {
            BotClient = new TelegramBotClient(botToken);
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text) && e.Message.NewChatMembers == null)
            {
                return;
            }

            foreach (var command in _commands.Where(command => command.Contains(e.Message)))
            {
                await command.Execute(e.Message);
                if (command is ForbiddenWordCommand)
                {
                    Log.Logger.Information($"{e.Message.From} нарушил правила чата: \"{e.Message.Text}\"");
                }
                else
                {
                    string commandIdentifier = command.Triggers != null
                        ? command.Triggers.First()
                        : command.GetType().Name;
                    Log.Logger.Information($"{e.Message.From} запросил команду {commandIdentifier}");
                }

                return;
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CallbackQuery.Data))
            {
                return;
            }

            foreach (var command in _callbackCommands.Where(command => command.Contains(e.CallbackQuery)))
            {
                try
                {
                    await command.Execute(e.CallbackQuery);
                    Log.Logger.Information(
                        $"{e.CallbackQuery.From} запросил callback команду {command.Triggers[0] ?? command.GetType().Name}");
                }
                catch (Exception ex)
                {
                    string errorTemplate = $"Unpredictable error occured ({nameof(ex)}): {ex.Message}\n" +
                                           $"Stack Trace: {ex.StackTrace}";
                    Log.Error(ex, errorTemplate);
                }

                return;
            }
        }

        private void InitializeListOfBotCommands(out List<Command> commands)
        {
            var helloWords = Program.Configuration["HelloWords"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var googleWords = Program.Configuration["GoogleWords"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            commands = new List<Command>
            {
                new HelpCommand(),
                new ListCommand(_chatService),
                new AddCommand(_chatService),
                new RemoveCommand(_chatService),
                new NotHelloCommand(helloWords),
                new GoogleCommand(googleWords),
                new ForbiddenWordCommand(_chatService),
                new ListOfPromotedMembersCommand(_chatService),
                new PromoteMemberCommand(_chatService),
                new UnpromoteMemberCommand(_chatService),
                new MuteCommand(_chatService),
                new UnmuteCommand(_chatService),
                new VerifyNewChatMemberCommand(),
                new DeleteMessageCommand(_chatService),
                new BanCommand(_chatService),
                new UnbanCommand(_chatService)
            };
        }

        private void InitializeListOfBotCallbackCommands(out List<CallbackCommand> commands)
        {
            commands = new List<CallbackCommand>
            {
                new ProveNewChatMemberCommand()
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
             {
                 try
                 {
                     StartPolling();
                     while (!cancellationToken.IsCancellationRequested)
                     {
                         Thread.Sleep(TimeSpan.FromMinutes(1));
                     }

                     StopPolling();
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine("Во время роботы случилась ошибка:");
                     Console.WriteLine(ex.Message);
                     Console.WriteLine(ex.Source);
                     Console.WriteLine(ex.StackTrace);
                     Console.WriteLine(ex.InnerException);
                 }
             }, cancellationToken);
        }

        private void StartPolling()
        {
            BotClient.StartReceiving();
            Log.Logger.Information("Бот начал обрабатывать сообщения...");
        }

        private void StopPolling()
        {
            BotClient.StopReceiving();
            Log.Logger.Information("Бот прекратил работу...");
            Log.CloseAndFlush();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(StopPolling, cancellationToken);
        }
    }
}