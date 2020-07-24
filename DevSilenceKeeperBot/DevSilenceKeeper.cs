using DevSilenceKeeperBot.Commands;
using DevSilenceKeeperBot.Commands.Callback;
using DevSilenceKeeperBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            _chatService = chatService;

            InitializeListOfBotCommands(out _commands);
            InitializeListOfBotCallbackCommands(out _callbackCommands);

            InitializeBotClientInstance(Program.Configuration["BotToken"]);
        }

        private static void InitializeBotClientInstance(string botToken)
        {
            BotClient = new TelegramBotClient(botToken);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessage(update.Message),
                UpdateType.EditedMessage => OnMessage(update.Message),
                UpdateType.CallbackQuery => OnCallbackQuery(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(_, exception, cancellationToken);
            }
        }

        private async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }

        private async Task HandleErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
        }

        private async Task OnMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.Text) && message.NewChatMembers == null)
            {
                return;
            }

            var chatAdmins = await BotClient.GetChatAdministratorsAsync(message.Chat.Id);
            if (chatAdmins.All(admin => admin.User.Id != BotClient.BotId))
            {
                return;
            }

            foreach (var command in _commands.Where(command => command.Contains(message)))
            {
                if (command is ForbiddenWordCommand)
                {
                    Log.Logger.Information($"{message.From} нарушил правила чата: \"{message.Text}\"");
                }
                else
                {
                    string commandIdentifier = command.Triggers != null
                        ? command.Triggers.First()
                        : command.GetType().Name;
                    Log.Logger.Information($"{message.From} запросил команду {commandIdentifier}");
                }

                await command.Execute(message);

                return;
            }
        }

        private async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            if (string.IsNullOrEmpty(callbackQuery.Data))
            {
                return;
            }

            foreach (var command in _callbackCommands.Where(command => command.Contains(callbackQuery)))
            {
                try
                {
                    Log.Logger.Information(
                        $"{callbackQuery.From} запросил callback команду {command.Triggers[0] ?? command.GetType().Name}");
                    await command.Execute(callbackQuery);
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
            var helloWords = Program.Configuration.GetSection("HelloWords").Get<string[]>();
            var googleWords = Program.Configuration.GetSection("GoogleWords").Get<string[]>();
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
                     StartPolling(cancellationToken);
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

        private void StartPolling(CancellationToken cancellationToken)
        {
            BotClient.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cancellationToken);
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