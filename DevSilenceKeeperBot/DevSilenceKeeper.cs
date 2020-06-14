using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using DevSilenceKeeperBot.Types;
using DevSilenceKeeperBot.Types.Settings;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                    caption: "[Непривет](https://neprivet.ru)",
                    parseMode: ParseMode.MarkdownV2,
                    replyToMessageId: e.Message.MessageId,
                    disableNotification: true);
            }
            else if (IsGoogleMessage(e.Message.Text))
            {
                string imageCaption = $"[{e.Message.Text}](https://www.google.com/search?q={e.Message.Text})\n" +
                    "[Пожалуйста, научитесь гуглить](http://sadykhzadeh.github.io/learn-to-google)";
                await _bot.SendPhotoAsync(
                    chatId: e.Message.Chat.Id,
                    photo: "https://sadykhzadeh.github.io/learn-to-google/img/bad-good.jpg",
                    caption: imageCaption,
                    parseMode: ParseMode.MarkdownV2,
                    replyToMessageId: e.Message.MessageId,
                    disableNotification: true);
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

        private string GetFormattedMessageText(string text)
        {
            var pattern = new Regex("[-:!@#$%^&*()}{|\":?><\\[\\]\\;'/.,~]");
            return pattern.Replace(text, "").ToLower();
        }
        private bool IsHelloMessage(string messageText)
        {
            const uint MaxWordsInHelloMesssage = 2;
            string[] words = GetFormattedMessageText(messageText).Split();
            return _settings.HelloWords.Any(word => words.Contains(word)) 
                && words.Length <= MaxWordsInHelloMesssage;
        }
        private bool IsGoogleMessage(string messageText)
        {
            const uint MinWordsGoogleMesssageLength = 3;
            string formattedText = GetFormattedMessageText(messageText);
            return _settings.GoogleWords.Any(phrase => formattedText.StartsWith(phrase))
                && formattedText.Split().Length >= MinWordsGoogleMesssageLength;
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