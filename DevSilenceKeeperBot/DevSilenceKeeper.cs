using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot
{
    internal sealed class DevSilenceKeeper
    {
        private readonly DbContext _context;
        private readonly ITelegramBotClient _bot;

        public DevSilenceKeeper(string token)
        {
            _context = new DbContext();
            _bot = new TelegramBotClient(token);
            _bot.OnMessage += OnMessage;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            bool isDeletedMessage = false;
            if (string.IsNullOrEmpty(e.Message.Text))
            {
                return;
            }

            string replyMessage = default;
            if (IsCommand(e.Message.Text))
            {
                Console.WriteLine($"{GetUserFullName(e.Message.From)} запросил команду: {e.Message.Text}");
                replyMessage = await ProcessCommand(e.Message);
            }
            else if (IsContainsForbiddenWord(e.Message))
            {
                if (await IsAdmin(e.Message.From, e.Message.Chat.Id))
                {
                    replyMessage = $"Модератор {GetUserFullName(e.Message.From)} нарушает правила чата!";
                }
                else
                {
                    replyMessage = await KickChatMember(e.Message);
                }

                await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                isDeletedMessage = true;
            }

            if (!string.IsNullOrEmpty(replyMessage))
            {
                try
                {
                    await _bot.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: replyMessage,
                        replyToMessageId: isDeletedMessage ? default : e.Message.MessageId);

                    Console.WriteLine(replyMessage);
                }
                catch (ApiRequestException)
                {
                    replyMessage = "Мышь успела удалить сообщение для reply\n(╯ರ ~ ರ)╯︵ ┻━┻.";
                    await _bot.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: replyMessage);
                }
            }
        }
        private bool IsCommand(Message message)
        {
            var chatForbiddenWords = _context.GetChatForbiddenWords(message.Chat.Id);
            if (chatForbiddenWords.Any(word => message.Text.Contains(word)))
            {
                return false;
            }
            return message.Text.StartsWith('/');
        }
        private async Task<string> ProcessCommand(Message message)
        {
            string[] words = message.Text
                .Replace("@devsilencekeeper_bot", string.Empty)
                .Substring(1)
                .Split();
            string command = words.First();
            string args = words.Length > 1 ? words[1] : null;
            string response;
            IEnumerable<string> chatForbiddenWords;
            switch (command)
            {
                case "words":
                case "templates":
                    string templates = string.Join('\n', _context.GetChatForbiddenWords(message.Chat.Id));
                    response = $"Cтроки-шаблоны в банлисте:\n{templates}";
                    break;
                case "add":
                    chatForbiddenWords = _context.GetChatForbiddenWords(message.Chat.Id);
                    if (!await IsAdmin(message.From, message.Chat.Id))
                    {
                        response = "Добавлять строки-шаблоны могут только модераторы!";
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
                    {
                        response = "Строка-шаблон должна состоять минимум из 4ех символов!";
                        break;
                    }

                    if (chatForbiddenWords?.Contains(args) == true)
                    {
                        response = "Дананя строка-шаблон уже присуствует в банлисте.";
                        break;
                    }

                    _context.AddChatForbiddenWord(message.Chat.Id, args);
                    response = $"Строка-шаблон \"{args}\" успешно добавлена";
                    break;
                case "remove":
                case "delete":
                case "del":
                case "rm":
                    chatForbiddenWords = _context.GetChatForbiddenWords(message.Chat.Id);

                    if (!await IsAdmin(message.From, message.Chat.Id))
                    {
                        response = "Удалять строки-шаблоны могут только модераторы!";
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
                    {
                        response = "Строка-шаблон должна состоять минимум из 4ех символов!";
                        break;
                    }

                    if (chatForbiddenWords?.Contains(args) == false)
                    {
                        response = "Дананя строка-шаблон отсуствует в банлисте.";
                        break;
                    }

                    _context.RemoveChatForbiddenWord(message.Chat.Id, args);
                    response = $"Строка-шаблон \"{args}\" успешно убрана";
                    break;
                case "help":
                    response = string.Format("Список комманд:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
                        "/help – показать помощь",
                        "/words – показать запрещенные строки-шаблоны",
                        "/templates – альтернатива /words",
                        "/add – добавить запрещенную строку-шаблон",
                        "/remove (/rm) – убрать запрещенную строку-шаблон",
                        "/delete (/del) – альтернатива /remove");
                    break;
                default:
                    response = default;
                    break;
            }

            return response;
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
            return $"Пользователь {GetUserFullName(message.From)} нарушил правила чата!";
        }
        private string GetUserFullName(User user)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                builder.Append(user.FirstName);
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                builder.Append($" {user.LastName}");
            }

            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                builder.Append($" (@{user.Username})");
            }

            return builder.ToString();
        }

        public void StartPolling()
        {
            _bot.StartReceiving();
            Console.WriteLine("Бот начал обрабатывать сообщения...");
        }
        public void StopPolling()
        {
            _bot.StopReceiving();
            Console.WriteLine("Бот прекратил работу...");
        }
    }
}