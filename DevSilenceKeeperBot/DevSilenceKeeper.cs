using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DevSilenceKeeperBot.Commands;
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
        private readonly TelegramBotClient _botClient;
        private readonly IChatService _chatService;

        private readonly List<Command> _commands;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public DevSilenceKeeper(
            IAppSettingsReader appSettingsReader,
            IChatService chatService,
            ILogger logger)
        {
            _settings = appSettingsReader.Read();
            _botClient = new TelegramBotClient(_settings.BotToken);
            _botClient.OnMessage += OnMessage;

            _chatService = chatService;
            _logger = logger;

            InitializeListOfBotCommands(out _commands);
        }

        public void Run()
        {
            try
            {
                StartPolling();
                while (!_tokenSource.IsCancellationRequested) Thread.Sleep(TimeSpan.FromMinutes(1));
                StopPolling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(value: "Во время роботы случилась ошибка:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
            }
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
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
                new UnmuteCommand(_chatService)
            };
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text))
            {
                return;
            }

            foreach (var command in _commands.Where(command => command.Contains(e.Message)))
            {
                await command.Execute(e.Message, _botClient).ConfigureAwait(false);
                _logger.Info(command is ForbiddenWordCommand
                    ? $"{e.Message.From} нарушил правила чата: \"{e.Message.Text}\""
                    : $"{e.Message.From} запросил команду {command.Triggers[0]}");

                return;
            }
        }

        private void StartPolling()
        {
            _botClient.StartReceiving();
            _logger.Info(text: "Бот начал обрабатывать сообщения...");
        }

        private void StopPolling()
        {
            _botClient.StopReceiving();
            _logger.Info(text: "Бот прекратил работу...");
        }
    }
}