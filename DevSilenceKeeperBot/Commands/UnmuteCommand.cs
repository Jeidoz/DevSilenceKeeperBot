using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class UnmuteCommand : Command
    {
        private readonly IChatService _chatService;
        public override string[] Triggers => new string[] { "/unmute" };

        public UnmuteCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Розмутить могут только модераторы и участники чата с привилегиями!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            if (isAdmin)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ничосе!. Админа розмутить пытаются...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            var unmutePermisions = new ChatPermissions
            {
                CanSendMessages = true,
                CanSendMediaMessages = true,
                CanSendOtherMessages = true,
                CanSendPolls = true
            };

            await botClient.RestrictChatMemberAsync(
                chatId: message.Chat.Id,
                userId: message.ReplyToMessage.From.Id,
                permissions: unmutePermisions,
                untilDate: DateTime.Now).ConfigureAwait(false);
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{message.ReplyToMessage.From} розмучен",
                replyToMessageId: message.MessageId).ConfigureAwait(false);
        }
    }
}