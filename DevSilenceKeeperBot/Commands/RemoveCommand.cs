using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class RemoveCommand : Command
    {
        private readonly IChatService _chatService;

        public RemoveCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/rm", "/remove"};

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if (!await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false))
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Добавлять строки-шаблоны могут только модераторы!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            string args = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .ToArray()
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Строка-шаблон должна состоять минимум из 4ех символов!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            var chatForbiddenWords = _chatService.GetChatForbiddenWords(message.Chat.Id);
            if (chatForbiddenWords?.Contains(args) == false)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Данная строка-шаблон отсутствует в банлисте.",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            _chatService.RemoveChatForbiddenWord(message.Chat.Id, args);
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                $"Строка-шаблон \"{args}\" успешно убрана",
                replyToMessageId: message.MessageId).ConfigureAwait(false);
        }
    }
}