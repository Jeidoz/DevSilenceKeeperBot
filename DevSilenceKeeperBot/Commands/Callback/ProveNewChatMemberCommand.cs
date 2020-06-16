using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands.Callback
{
    public class ProveNewChatMemberCommand : CallbackCommand
    {
        public override string[] Triggers => new[] {"verified"};
        
        public override async Task Execute(CallbackQuery query, TelegramBotClient botClient)
        {
            if (query.From.Id != InvokerId)
            {
                return;
            }
            
            var unmutePermissions = new ChatPermissions
            {
                CanSendMessages = true,
                CanSendMediaMessages = true,
                CanSendOtherMessages = true,
                CanSendPolls = true
            };

            await botClient.RestrictChatMemberAsync(
                query.Message.Chat.Id,
                query.From.Id,
                unmutePermissions,
                DateTime.Now).ConfigureAwait(false);
            
            
            botClient.SendTextMessageAsync(
                query.Message.Chat.Id,
                $"[{query.From}](tg://user?id={query.From.Id}) теперь ты полноценный участник чата!",
                ParseMode.Markdown)
                .Wait();

            await botClient.DeleteMessageAsync(
                query.Message.Chat.Id,
                query.Message.MessageId);
        }
    }
}