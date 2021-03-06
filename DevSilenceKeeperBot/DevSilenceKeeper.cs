﻿using DevSilenceKeeperBot.Commands;
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
using Telegram.Bot.Args;
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
            BotClient.OnMessage += OnMessage;
            BotClient.OnMessageEdited += OnMessage;
            BotClient.OnCallbackQuery += OnCallbackQuery;
        }

        private static void InitializeBotClientInstance(string botToken)
        {
            BotClient = new TelegramBotClient(botToken);
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (!IsValidChatType(e.Message.Chat.Type))
            {
                await BotClient.SendTextMessageAsync(
                    e.Message.Chat.Id,
                    "Бот не работает вне груповых чатов.",
                    replyToMessageId: e.Message.MessageId);
                return;
            }
            
            if (string.IsNullOrEmpty(e.Message.Text) && e.Message.NewChatMembers == null)
            {
                return;
            }

            var chatAdmins = await BotClient.GetChatAdministratorsAsync(e.Message.Chat.Id);
            if (chatAdmins.All(admin => admin.User.Id != BotClient.BotId))
            {
                return;
            }

            foreach (var command in _commands.Where(command => command.Contains(e.Message)))
            {
                if (command is ForbiddenWordCommand)
                {
                    Log.Logger.Information($"{e.Message.From} нарушил правила чата: \"{e.Message.Text}\"");
                }
                else
                {
                    string commandIdentifier = command.Triggers != null
                        ? command.Triggers.First()
                        : command.GetType().Name;
                    Log.Logger.Information($"{e.Message.From} (@{e.Message.Chat.Username}) запросил команду {commandIdentifier}: {e.Message.Text}");
                }

                try
                {
                    await command.Execute(e.Message);
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

        private bool IsValidChatType(ChatType currentChatType)
        {
            return currentChatType == ChatType.Group || currentChatType == ChatType.Supergroup;
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CallbackQuery.Data))
            {
                return;
            }

            var chatAdmins = await BotClient.GetChatAdministratorsAsync(e.CallbackQuery.Message.Chat.Id);
            if (chatAdmins.All(admin => admin.User.Id != BotClient.BotId))
            {
                return;
            }

            foreach (var command in _callbackCommands.Where(command => command.Contains(e.CallbackQuery)))
            {
                try
                {
                    Log.Logger.Information(
                        $"{e.CallbackQuery.From} запросил callback команду {command.Triggers[0] ?? command.GetType().Name}: {e.CallbackQuery.Data}");
                    await command.Execute(e.CallbackQuery);
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
            BotClient.StartReceiving(cancellationToken: cancellationToken);
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