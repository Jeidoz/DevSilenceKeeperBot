using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public class UnbanCommand : Command
    {
        private readonly IChatService _chatService;

        public UnbanCommand(IChatService chatService)
        {
            _chatService = chatService;
        }
        
        public override string[] Triggers => new[] {"/unban"};
        public override async Task Execute(Message message)
        {
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Розбанить могут только модераторы и участники чата с привилегиями!",
                    replyToMessageId: message.MessageId);
                return;
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id);
            if (isAdmin)
            {
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Ничосе!. Админа розбанить пытаются...",
                    replyToMessageId: message.MessageId);
                return;
            }

            await DevSilenceKeeper.BotClient.UnbanChatMemberAsync(
                message.Chat.Id,
                message.ReplyToMessage.From.Id);
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                $"{message.ReplyToMessage.From} розбанен",
                replyToMessageId: message.MessageId);
        }
    }
}