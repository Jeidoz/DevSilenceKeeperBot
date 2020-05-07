using DevSilenceKeeperBot.Types;
using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Helpers
{
    public sealed class CommandHelper
    {
        private readonly DbContext _context;

        public CommandHelper(DbContext context)
        {
            _context = context;
        }

        public bool IsCommand(Message message)
        {
            return message.Entities?.Any(entity => entity.Type == MessageEntityType.BotCommand) == true;
        }

        public Command IdentifyCommand(Message message)
        {
            string[] words = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string commandLabel = words.First();
            if (commandLabel.Contains('@'))
            {
                commandLabel = commandLabel.Split('@').First();
            }
            commandLabel = commandLabel.Substring(1);

            return new Command(message.Chat.Id, commandLabel, words.Skip(1).ToArray());
        }

        public string GetCommandResponseMessage(ref Command command)
        {
            string response;
            switch (command.Label)
            {
                case "list":
                case "words":
                case "templates":
                    response = DoListCommand(ref command.ChatId);
                    break;
                case "add":
                    if (command.IsInvokerHasAdminRights)
                    {
                        response = DoAddCommand(ref command.ChatId, command.Arguments?.FirstOrDefault());
                        break;
                    }
                    response = "Добавлять строки-шаблоны могут только модераторы!";
                    break;
                case "remove":
                case "delete":
                case "del":
                case "rm":
                    if (command.IsInvokerHasAdminRights)
                    {
                        response = DoRemoveCommand(ref command.ChatId, command.Arguments?.FirstOrDefault());
                        break;
                    }
                    response = "Удалять строки-шаблоны могут только модераторы!";
                    break;
                case "help":
                    response = DoHelpCommand();
                    break;
                default:
                    response = default;
                    break;
            }

            return response;
        }
        private string DoListCommand(ref long chatId)
        {
            var chatForbiddenWords = _context.GetChatForbiddenWords(chatId);
            if (chatForbiddenWords?.Count() > 0)
            {
                string templates = string.Join('\n', chatForbiddenWords);
                return $"Cтроки-шаблоны в банлисте:\n{templates}";
            }
            return $"Банлист пуст.";
        }
        private string DoAddCommand(ref long chatId, string args)
        {
            if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
            {
                return "Строка-шаблон должна состоять минимум из 4ех символов!";
            }

            var chatForbiddenWords = _context.GetChatForbiddenWords(chatId);
            if (chatForbiddenWords?.Contains(args) == true)
            {
                return "Даная строка-шаблон уже присуствует в банлисте.";
            }

            _context.AddChatForbiddenWord(chatId, args);
            return $"Строка-шаблон \"{args}\" успешно добавлена";
        }
        private string DoRemoveCommand(ref long chatId, string args)
        {
            if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
            {
                return "Строка-шаблон должна состоять минимум из 4ех символов!";
            }

            var chatForbiddenWords = _context.GetChatForbiddenWords(chatId);
            if (chatForbiddenWords?.Contains(args) == false)
            {
                return "Даная строка-шаблон отсуствует в банлисте.";
            }

            _context.RemoveChatForbiddenWord(chatId, args);
            return $"Строка-шаблон \"{args}\" успешно убрана";
        }
        private string DoHelpCommand()
        {
            return string.Format("Список комманд:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
                        "/help – показать помощь",
                        "/words – показать запрещенные строки-шаблоны",
                        "/templates – альтернатива /words",
                        "/add – добавить запрещенную строку-шаблон",
                        "/remove (/rm) – убрать запрещенную строку-шаблон",
                        "/delete (/del) – альтернатива /remove");
        }
    }
}