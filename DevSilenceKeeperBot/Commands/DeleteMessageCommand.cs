using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public class DeleteMessageCommand : Command
    {
        private readonly IChatService _chatService;

        public DeleteMessageCommand(IChatService chatService)
        {
            _chatService = chatService;
            Triggers = new[] {"/del", "/delete"};
        }

        public override string[] Triggers { get; }

        public override async Task Execute(Message message)
        {
            if (message.ReplyToMessage is null)
            {
                await SendTextResponse("И что ты собрался удалять? Реплаем вызывай, что бы я знал что удалять!",
                    message);
                return;
            }

            var dateDiffByUtc = DateTime.Now.ToUniversalTime() - message.ReplyToMessage.Date.ToUniversalTime();
            bool isOlderThanTwoDaysMessage = dateDiffByUtc > TimeSpan.FromDays(2);
            if (isOlderThanTwoDaysMessage)
            {
                await SendTextResponse(
                    "Уже поздно удалять! Могу только в течении двох суток удалить, а дальше \"Интернет запомнит всё\"...",
                    message);
                return;
            }

            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await SendTextResponse("Удалять сообщения могут только модераторы и участники чата с привилегиями!",
                    message);
                return;
            }

            await DevSilenceKeeper.BotClient.DeleteMessageAsync(message.Chat.Id, message.ReplyToMessage.MessageId);
            await DevSilenceKeeper.BotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }

        private static async Task SendTextResponse(string text, Message replyToMessage)
        {
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    replyToMessage.Chat.Id,
                    text,
                    replyToMessageId: replyToMessage.MessageId)
                ;
        }
    }
}