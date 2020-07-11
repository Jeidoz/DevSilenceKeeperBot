using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
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

        public override async Task Execute(Message message)
        {
            if (!await message.From.IsAdmin(message.Chat.Id))
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Добавлять строки-шаблоны могут только модераторы!",
                    replyToMessageId: message.MessageId);
                return;
            }

            string args = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .ToArray()
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(args) || args.Length < 4)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Строка-шаблон должна состоять минимум из 4ех символов!",
                    replyToMessageId: message.MessageId);
                return;
            }

            var chatForbiddenWords = await _chatService.GetForbiddenWordsAsync(message.Chat.Id);
            if (chatForbiddenWords?.Contains(args) == false)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Данная строка-шаблон отсутствует в банлисте.",
                    replyToMessageId: message.MessageId);
                return;
            }

            await _chatService.RemoveForbiddenWordAsync(message.Chat.Id, args);
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                $"Строка-шаблон \"{args}\" успешно убрана",
                replyToMessageId: message.MessageId);
        }
    }
}