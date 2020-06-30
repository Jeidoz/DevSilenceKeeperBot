using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ListOfPromotedMembersCommand : Command
    {
        private readonly IChatService _chatService;

        public ListOfPromotedMembersCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/promoted", "/users"};

        public override async Task Execute(Message message)
        {
            var promotedMemberIds = _chatService.GetPromotedMembers(message.Chat.Id);
            var promotedMembers = new List<string>();
            if (promotedMemberIds != null)
            {
                promotedMembers.AddRange(promotedMemberIds
                    .Select(member => $"[@{member.Username}](tg://user?id={member.UserId})"));
            }

            string response;
            if (promotedMembers.Count > 0)
            {
                response = "Участники чата с привилегиями:\n\n" + string.Join('\n', promotedMembers);
            }
            else
            {
                response = "В данном чате отсутствуют участники с привилегиями.";
            }

            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Markdown);
        }
    }
}