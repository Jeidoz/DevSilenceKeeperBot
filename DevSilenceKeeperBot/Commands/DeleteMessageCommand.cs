using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot;
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
        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if (message.ReplyToMessage is null)
            {
                await SendTextResponse("И что ты собрался удалять? Реплаем вызывай, что бы я знал что удалять!",
                    message, botClient);
                return;
            }

            TimeSpan dateDiffByUtc = DateTime.Now.ToUniversalTime() - message.ReplyToMessage.Date.ToUniversalTime(); 
            bool isOlderThanTwoDaysMessage = dateDiffByUtc > TimeSpan.FromDays(2);
            if (isOlderThanTwoDaysMessage)
            {
                await SendTextResponse("Уже поздно удалять! Могу только в течении двох суток удалить, а дальше \"Интернет запомнит всё\"...",
                    message, botClient);
                return;
            }
            
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await SendTextResponse("Удалять сообщения могут только модераторы и участники чата с привилегиями!",
                    message, botClient);
                return;
            }

            await botClient.DeleteMessageAsync(message.Chat.Id, message.ReplyToMessage.MessageId);
        }
        
        private static async Task SendTextResponse(string text, Message replyToMessage, TelegramBotClient botClient)
        {
            await botClient.SendTextMessageAsync(
                    replyToMessage.Chat.Id,
                    text,
                    replyToMessageId: replyToMessage.MessageId)
                .ConfigureAwait(false);
        }
    }
}