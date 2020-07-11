using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class UnmuteCommand : Command
    {
        private readonly IChatService _chatService;

        public UnmuteCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/unmute"};

        public override async Task Execute(Message message)
        {
            var promotedMembers = await _chatService.GetPromotedMembersAsync(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Розмутить могут только модераторы и участники чата с привилегиями!",
                    replyToMessageId: message.MessageId);
                return;
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id);
            if (isAdmin)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Ничосе!. Админа розмутить пытаются...",
                    replyToMessageId: message.MessageId);
                return;
            }

            var unmutePermissions = new ChatPermissions
            {
                CanSendMessages = true,
                CanSendMediaMessages = true,
                CanSendOtherMessages = true,
                CanSendPolls = true,
                CanAddWebPagePreviews = true,
                CanInviteUsers = true
            };

            await DevSilenceKeeper.BotClient.RestrictChatMemberAsync(
                message.Chat.Id,
                message.ReplyToMessage.From.Id,
                unmutePermissions,
                DateTime.Now);
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                $"{message.ReplyToMessage.From} розмучен",
                replyToMessageId: message.MessageId);
        }
    }
}