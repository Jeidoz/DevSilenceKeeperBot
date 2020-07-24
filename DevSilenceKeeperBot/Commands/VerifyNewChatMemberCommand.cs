using DevSilenceKeeperBot.Commands.Callback.Data;
using DevSilenceKeeperBot.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevSilenceKeeperBot.Commands
{
    public class VerifyNewChatMemberCommand : Command
    {
        private readonly TimeSpan _timeLimitForVerification = TimeSpan.FromMinutes(3);

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

                var callbackData = new CallbackQueryData
                {
                    UserId = newChatMember.Id,
                    Trigger = "verified",
                    Arguments = new[]
                    {
                        message.MessageId.ToString()
                    }
                };
                var captchaMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Я не бот", callbackData.ToString())
                    });

                var verifyMessage = await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"{newChatMember} Нажми на кнопку, что бы получить возможность отсылать сообщения.",
                    replyToMessageId: message.MessageId,
                    replyMarkup: captchaMarkup);

                await Task.Run(async () =>
                {
                    Thread.Sleep(_timeLimitForVerification);
                    try
                    {
                        await DevSilenceKeeper.BotClient.DeleteMessageAsync(verifyMessage.Chat.Id, verifyMessage.MessageId);
                        await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                            message.Chat.Id,
                            $"{newChatMember} не прошел проверку на бота, за отведенное время ({_timeLimitForVerification.TotalSeconds} c.) и был кикнут из чата.",
                            replyToMessageId: message.MessageId);
                        await DevSilenceKeeper.BotClient.KickChatMemberAsync(message.Chat.Id, newChatMember.Id);
                        await DevSilenceKeeper.BotClient.UnbanChatMemberAsync(message.Chat.Id, newChatMember.Id);
                    }
                    catch
                    {
                        // User complete verification, because can't delete not existing message
                    }
                });
            }
        }
    }
}