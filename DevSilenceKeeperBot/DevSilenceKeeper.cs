using DevSilenceKeeperBot.Commands;
using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using DevSilenceKeeperBot.Types.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace DevSilenceKeeperBot
{
    public sealed class DevSilenceKeeper : IDevSilenceKeeper
    {
        private readonly TelegramBotClient _botClient;
        private readonly IChatService _chatService;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;

        private readonly List<Command> _commands;

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
                new UnpromoteMemberCommand(_chatService, _logger)
            };
        }

        public void Run()
        {
            try
            {
                StartPolling();
                Console.WriteLine("Введите \"stop\" что бы остановить бота.");
                do
                {
                    if (Console.ReadLine().ToLower() == "stop")
                    {
                        break;
                    }
                }
                while (true);
                StopPolling();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Во время роботы случилась ошибка:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return;
            }
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text))
            {
                return;
            }

            foreach(var command in _commands)
            {
                if(command.Contains(e.Message))
                {
                    await command.Execute(e.Message, _botClient);
                    if (command is ForbiddenWordCommand)
                    {
                        _logger.Info($"{e.Message.From} нарушил правила чата: \"{e.Message.Text}\"");
                    }
                    else
                    {
                        _logger.Info($"{e.Message.From} запросил команду {command.Triggers.First()}");
                    }
                    return;
                }
            }
        }

        private void StartPolling()
        {
            _botClient.StartReceiving();
            _logger.Info("Бот начал обрабатывать сообщения...");
        }
        private void StopPolling()
        {
            _botClient.StopReceiving();
            _logger.Info("Бот прекратил работу...");
        }
    }
}