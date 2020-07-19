using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands.Callback
{
    public class ProveNewChatMemberCommand : CallbackCommand
    {
        public override string[] Triggers => new[] { "verified" };

        public override async Task Execute(CallbackQuery query)
        {
            if (query.From.Id != InvokerId)
            {
                var expectedChatMember = await DevSilenceKeeper.BotClient.GetChatMemberAsync(query.Message.Chat.Id, InvokerId);
                await DevSilenceKeeper.BotClient.AnswerCallbackQueryAsync(
                    query.Id,
                    $"Эту кнопку должен нажать определенный участник чата — {expectedChatMember.User}.",
                    true);
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
                query.Message.Chat.Id,
                query.From.Id,
                unmutePermissions,
                DateTime.Now);

            DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    query.Message.Chat.Id,
                    $"[{query.From}](tg://user?id={query.From.Id}) теперь ты полноценный участник чата!",
                    ParseMode.Markdown)
                .Wait();

            await DevSilenceKeeper.BotClient.DeleteMessageAsync(
                query.Message.Chat.Id,
                query.Message.MessageId);
        }
    }
}