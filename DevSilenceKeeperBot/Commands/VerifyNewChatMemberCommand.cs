using System;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevSilenceKeeperBot.Commands
{
    public class VerifyNewChatMemberCommand : Command
    {
        public VerifyNewChatMemberCommand()
        {
            Triggers = null;
        }

        public override string[] Triggers { get; }

        public override bool Contains(Message message)
        {
            if (message.NewChatMembers == null)
            {
                return false;
            }

            return message.NewChatMembers.Length > 0;
        }

        public override async Task Execute(Message message)
        {
            if (await message.From.IsMuted(message.Chat.Id))
            {
                var chatMember = await DevSilenceKeeper.BotClient.GetChatMemberAsync(message.Chat.Id, message.From.Id);
                string replyText =
                    $"С возвращением в чат! Ты ещё в муте до {chatMember.UntilDate:dd.MM.yyyy HH:mm:ss} (UTC+02:00)";
                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    replyText,
                    replyToMessageId: message.MessageId);
                return;
            }

            await DevSilenceKeeper.BotClient.RestrictChatMemberAsync(
                message.Chat.Id,
                message.From.Id,
                new ChatPermissions {CanSendMessages = false},
                DateTime.Now);

            var captchaMarkup = new InlineKeyboardMarkup(
                new[]
                {
                    // TODO Callback data class with implicit cast to string
                    InlineKeyboardButton.WithCallbackData("Я не бот", $"{message.From.Id}:verified:{message.MessageId}")
                });

            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                "Нажми на кнопку, что бы получить возможность отсылать сообщения.",
                replyToMessageId: message.MessageId,
                replyMarkup: captchaMarkup);
        }
    }
}