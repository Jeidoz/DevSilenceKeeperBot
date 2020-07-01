using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DevSilenceKeeperBot.Commands;
using DevSilenceKeeperBot.Commands.Callback;
using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using DevSilenceKeeperBot.Types.Settings;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace DevSilenceKeeperBot
{
    // ReSharper disable once UnusedType.Global
    public sealed class DevSilenceKeeper : IDevSilenceKeeper
    {
        public static ITelegramBotClient BotClient;

        private readonly AppSettings _settings;
        private readonly IChatService _chatService;
        private readonly ILogger _logger;
        
        private readonly List<Command> _commands;
        private readonly List<CallbackCommand> _callbackCommands;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public DevSilenceKeeper(
            IAppSettingsReader appSettingsReader,
            IChatService chatService,
            ILogger logger)
        {
            _settings = appSettingsReader.Read();
            InitializeBotClientInstance(_settings.BotToken);
            BotClient.OnMessage += OnMessage;
            BotClient.OnCallbackQuery += OnCallbackQuery;

            _chatService = chatService;
            _logger = logger;

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
                    _logger.Info($"{e.Message.From} нарушил правила чата: \"{e.Message.Text}\"");
                }
                else
                {
                    string commandIdentifier = command.Triggers != null
                        ? command.Triggers.First()
                        : command.GetType().Name;
                    _logger.Info($"{e.Message.From} запросил команду {commandIdentifier}");
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
                await command.Execute(e.CallbackQuery);
                _logger.Info(
                    $"{e.CallbackQuery.From} запросил callback команду {command.Triggers[0] ?? command.GetType().Name}");
                return;
            }
        }
        private void InitializeListOfBotCommands(out List<Command> commands)
        {
            commands = new List<Command>
            {
                new HelpCommand(),
                new ListCommand(_chatService),
                new AddCommand(_chatService),
                new RemoveCommand(_chatService),
                new NotHelloCommand(_settings.HelloWords),
                new GoogleCommand(_settings.GoogleWords),
                new ForbiddenWordCommand(_chatService),
                new ListOfPromotedMembersCommand(_chatService),
                new PromoteMemberCommand(_chatService, _logger),
                new UnpromoteMemberCommand(_chatService, _logger),
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
        
        public void Run()
        {
            try
            {
                StartPolling();
                while (!_tokenSource.IsCancellationRequested)
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
        }
        private void StartPolling()
        {
            BotClient.StartReceiving();
            _logger.Info("Бот начал обрабатывать сообщения...");
        }
        private void StopPolling()
        {
            BotClient.StopReceiving();
            _logger.Info("Бот прекратил работу...");
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }
    }
}