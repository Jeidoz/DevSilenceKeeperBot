using System;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class UnpromoteMemberCommand : Command
    {
        private readonly IChatService _chatService;

        public UnpromoteMemberCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/unpromote"};

        public override async Task Execute(Message message)
        {
            if (!await message.From.IsAdmin(message.Chat.Id))
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Надавать привилегии могут только модераторы!",
                    replyToMessageId: message.MessageId);
                return;
            }

            if (message.ReplyToMessage is null)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Для понижения привилегий, нужно делать Reply на сообщение участника чата.",
                    replyToMessageId: message.MessageId);
                return;
            }

            if (message.ReplyToMessage.From == message.From)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Понижать самого себя любимого как-то неправильно...",
                    replyToMessageId: message.MessageId);
                return;
            }

            string response;
            string usernameMarkup =
                $"[@{message.ReplyToMessage.From.Username}](tg://user?id={message.ReplyToMessage.From.Id})";
            try
            {
                await _chatService.RemovePromotedMemberAsync(message.Chat.Id, message.ReplyToMessage.From.Id);
                response = $"{usernameMarkup}, потерял свои привилегии\\!";
            }
            catch (RemovingNotExistingRecordException)
            {
                response = $"{usernameMarkup}, ты и так обычный смертный ಠ\\_ಠ";
                Log.Logger.Warning(
                    $"[{nameof(RemovingNotExistingRecordException)}]: Chat: {message.Chat}; Invoker: {message.From}; Text: \"{message.Text}\"");
            }
            catch (Exception ex)
            {
                response = $"{usernameMarkup}, извини, я сломался. Пиши создателю.";
                Log.Logger.Error($"[{nameof(ex)}]: {ex.Message}\n{ex.StackTrace}");
            }

            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Markdown);
        }
    }
}