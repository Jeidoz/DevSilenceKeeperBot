using DevSilenceKeeperBot.Extensions;
using System;
using System.Threading.Tasks;
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
            foreach (var newChatMember in message.NewChatMembers)
            {
                if (await newChatMember.IsMuted(message.Chat.Id))
                {
                    var chatMember = await DevSilenceKeeper.BotClient.GetChatMemberAsync(message.Chat.Id, newChatMember.Id);
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
                    newChatMember.Id,
                    new ChatPermissions { CanSendMessages = false },
                    DateTime.Now);

                var captchaMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                    // TODO Callback data class with implicit cast to string
                    InlineKeyboardButton.WithCallbackData("Я не бот",
                        $"{newChatMember.Id}:verified:{message.MessageId}")
                    });

                await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"{newChatMember} Нажми на кнопку, что бы получить возможность отсылать сообщения.",
                    replyToMessageId: message.MessageId,
                    replyMarkup: captchaMarkup);
            }
        }
    }
}