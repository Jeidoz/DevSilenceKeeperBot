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
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient);
            bool isPromotedChatMember = promotedMembers.Any(member => member.UserId == message.From.Id);
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Розмутить могут только модераторы и учасники чата с привилегиями!",
                    replyToMessageId: message.MessageId);
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id, botClient);
            if (isAdmin)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ничосе!. Админа раззамутить пытаются...",
                    replyToMessageId: message.MessageId);
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
                untilDate: DateTime.Now);
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{message.ReplyToMessage.From} розмучен",
                replyToMessageId: message.MessageId);
        }
    }
}