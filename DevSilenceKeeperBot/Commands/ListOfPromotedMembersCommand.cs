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
            if (promotedMemberIds != null)
            {
                foreach (var member in promotedMemberIds)
                {
                    promotedMembers.Add($"[@{member.Username}](tg://user?id={member.UserId})");
                }
            }

            string response;
            if (promotedMembers.Count > 0)
            {
                response = "Учасники чата с привилегиями:\n\n" + string.Join('\n', promotedMembers);
            }
            else
            {
                response = "В данном чате отсуствуют учасники с привилегиями\\.";
            }
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Markdown);
        }
    }
}
