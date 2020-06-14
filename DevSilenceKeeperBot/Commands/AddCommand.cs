using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class AddCommand : Command
    {
        private readonly IChatService _chatService;
        public override string[] Triggers => new string[] { "/add" };

        public AddCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if(await message.From.IsAdmin(message.Chat.Id, botClient) == false)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Добавлять строки-шаблоны могут только модераторы!",
                    replyToMessageId: message.MessageId);
                return;
            }

            string args = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .ToArray()
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Строка-шаблон должна состоять минимум из 4ех символов!",
                    replyToMessageId: message.MessageId);
                return;
            }

            var chatForbiddenWords = _chatService.GetChatForbiddenWords(message.Chat.Id);
            if (chatForbiddenWords?.Contains(args) == true)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Даная строка-шаблон уже присуствует в банлисте.",
                    replyToMessageId: message.MessageId);
                return;
            }

            _chatService.AddChatForbiddenWord(message.Chat.Id, args);
            await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Строка-шаблон \"{args}\" успешно добавлена",
                    replyToMessageId: message.MessageId);
        }
    }
}