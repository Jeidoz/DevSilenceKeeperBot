using DevSilenceKeeperBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ListOfPromotedMembersCommand : Command
    {
        private readonly IChatService _chatService;
        public override string[] Triggers => new string[] { "/promoted", "/users" };

        public ListOfPromotedMembersCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var promotedMemberIds = _chatService.GetPromotedMembers(message.Chat.Id);
            List<string> promotedMembers = new List<string>();
            foreach(var member in promotedMemberIds)
            {
                promotedMembers.Add($"[@{member.Username} \\({member.UserId}\\)](tg://user?id={member.UserId})");
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', promotedMembers),
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.MarkdownV2);
        }
    }
}
