using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using DevSilenceKeeperBot.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot
{
    public sealed class DevSilenceKeeper : IDevSilenceKeeper
    {
        private readonly ITelegramBotClient _bot;
        private readonly IChatService _chatService;
        private readonly CommandHelper _commandHelper;

        public DevSilenceKeeper(IAppSettingsReader appSettingsReader, IChatService chatService)
        {
            var settings = appSettingsReader.Read();
            _bot = new TelegramBotClient(settings.BotToken);
            _chatService = chatService;
            _commandHelper = new CommandHelper(_chatService);
            _bot.OnMessage += OnMessage;
        }

        public void Run()
        {
            try
            {
                StartPolling();
                Console.WriteLine("Введите \"stop\" что бы остановить бота.");
                do
                {
                    if (Console.ReadLine() == "stop")
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

            string replyMessage = default;
            if (_commandHelper.IsCommand(e.Message))
            {
                replyMessage = await ProcessCommand(e.Message);
            }
            else if (IsContainsForbiddenWord(e.Message))
            {
                replyMessage = await ProcessMessageWithForbiddenWord(e.Message);
            }

            await SendReplyMessage(replyMessage, e.Message.Chat.Id);
        }
        private async Task<string> ProcessCommand(Message message)
        {
            ConsoleLogger.Info($"{message.From} запросил команду: {message.Text}");

            Command command = _commandHelper.IdentifyCommand(message);
            command.IsInvokerHasAdminRights = await IsAdmin(message.From, message.Chat.Id);

            return _commandHelper.GetCommandResponseMessage(ref command);
        }

        private bool IsContainsForbiddenWord(Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            var chatForbiddenWords = _chatService.GetChatForbiddenWords(message.Chat.Id);

            if(chatForbiddenWords == null || chatForbiddenWords.Count() == 0)
            {
                return false;
            }

            if (message.Entities != null)
            {
                if (message.Entities
                    .Any(entity => chatForbiddenWords
                        .Any(word => entity.Url?.Contains(word) == true)))
                {
                    return true;
                }
            }

            return chatForbiddenWords.Any(word => message.Text.Contains(word));
        }
        private async Task<string> ProcessMessageWithForbiddenWord(Message message)
        {
            await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            if (await IsAdmin(message.From, message.Chat.Id))
            {
                return $"Модератор {message.From} нарушает правила чата!";
            }
            return await KickChatMember(message);
        }
        private async Task<bool> IsAdmin(User sender, long chatId)
        {
            var chatAdmins = await _bot.GetChatAdministratorsAsync(chatId);
            return chatAdmins.Any(member => member.User.Id == sender.Id);
        }
        private async Task<string> KickChatMember(Message message)
        {
            DateTime until = DateTime.Now.AddSeconds(31);
            await _bot.KickChatMemberAsync(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                untilDate: until);
            return $"Пользователь {message.From} нарушил правила чата!";
        }

        private async Task SendReplyMessage(string replyMessage, long chatId)
        {
            if (!string.IsNullOrEmpty(replyMessage))
            {
                await _bot.SendTextMessageAsync(chatId, replyMessage);
            }
        }

        private void StartPolling()
        {
            _bot.StartReceiving();
            ConsoleLogger.Info("Бот начал обрабатывать сообщения...");
        }
        private void StopPolling()
        {
            _bot.StopReceiving();
            ConsoleLogger.Info("Бот прекратил работу...");
        }
    }
}