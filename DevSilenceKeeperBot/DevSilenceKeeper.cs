using DevSilenceKeeperBot.Helpers;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot
{
    internal sealed class DevSilenceKeeper
    {
        private readonly DbContext _context;
        private readonly CommandHelper _commandHelper;
        private readonly ITelegramBotClient _bot;

        public DevSilenceKeeper(string token)
        {
            _context = new DbContext();
            _commandHelper = new CommandHelper(_context);
            _bot = new TelegramBotClient(token);
            _bot.OnMessage += OnMessage;
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

            var chatForbiddenWords = _context.GetChatForbiddenWords(message.Chat.Id);

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

        public void StartPolling()
        {
            _bot.StartReceiving();
            ConsoleLogger.Info("Бот начал обрабатывать сообщения...");
        }
        public void StopPolling()
        {
            _bot.StopReceiving();
            ConsoleLogger.Info("Бот прекратил работу...");
        }
    }
}