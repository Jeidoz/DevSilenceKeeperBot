using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class PromoteMemberCommand : Command
    {
        private readonly IChatService _chatService;

        public PromoteMemberCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/promote"};

        public override async Task Execute(Message message)
        {
            var promotedMembers = await _chatService.GetPromotedMembersAsync(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
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
                    "Для повышения привилегий, нужно делать Reply на сообщение участника чата.",
                    replyToMessageId: message.MessageId);
                return;
            }

            if (message.ReplyToMessage.From == message.From)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Повышать самого себя любимого как-то неправильно...",
                    replyToMessageId: message.MessageId);
                return;
            }

            string response;
            string usernameMarkup =
                $"[@{message.ReplyToMessage.From.Username}](tg://user?id={message.ReplyToMessage.From.Id})";
            try
            {
                await _chatService.AddPromotedMemberAsync(message.Chat.Id, message.ReplyToMessage.From);
                response = $"{usernameMarkup}, поздравляю с повышением!";
            }
            catch (AddingDuplicateRecord)
            {
                response = $"{usernameMarkup}, ты уже привилегирован ಠ\\_ಠ";
                Log.Logger.Warning(
                    $"[{nameof(AddingDuplicateRecord)}]: Chat: {message.Chat}; Invoker: {message.From}; Text: \"{message.Text}\"");
            }
            catch (Exception ex)
            {
                response = $"{usernameMarkup}, извини, я сломался. Пиши создателю.";
                Log.Logger.Warning($"[{nameof(ex)}]: {ex.Message}\n{ex.StackTrace}");
            }

            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Markdown);
        }
    }
}