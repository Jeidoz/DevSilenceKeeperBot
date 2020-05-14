using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using DevSilenceKeeperBot.Types;
using DevSilenceKeeperBot.Types.Settings;
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
        private readonly AppSettings _settings;

        public DevSilenceKeeper(IAppSettingsReader appSettingsReader, IChatService chatService)
        {
            _settings = appSettingsReader.Read();
            _bot = new TelegramBotClient(_settings.BotToken);
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
            else if (IsHelloMessage(e.Message.Text))
            {
                await _bot.SendPhotoAsync(
                    chatId: e.Message.Chat.Id,
                    photo: "https://neprivet.ru/img/bad-good.png",
                    caption: "https://neprivet.ru");
            }

            if (!string.IsNullOrEmpty(replyMessage))
            {
                await _bot.SendTextMessageAsync(
                    chatId: e.Message.Chat.Id, 
                    text: replyMessage, 
                    replyToMessageId: e.Message.MessageId);
            }
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

        private bool IsHelloMessage(string messageText)
        {
            string[] messageWords = Array.ConvertAll(
                messageText.ToLower().Split(' ', options: StringSplitOptions.RemoveEmptyEntries),
                p => p.Trim(new char[] { ',', '.', '!' }));
            return _settings.HelloWords.Any(word => messageWords.Contains(word)) 
                && messageWords.Length < 3;
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