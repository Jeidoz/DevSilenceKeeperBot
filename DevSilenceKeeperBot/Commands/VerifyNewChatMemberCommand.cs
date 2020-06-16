﻿using System;
using System.Threading.Tasks;
using Telegram.Bot;
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
            return message.NewChatMembers.Length > 0;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            await botClient.RestrictChatMemberAsync(
                message.Chat.Id,
                message.From.Id,
                new ChatPermissions {CanSendMessages = false},
                DateTime.Now).ConfigureAwait(false);
            
            var captchaMarkup = new InlineKeyboardMarkup(
                new[]
                {
                    // TODO Callback data class with implicit cast to string
                    InlineKeyboardButton.WithCallbackData(text: "Я не бот", $"{message.From.Id}:verified:{message.MessageId}"), 
                });
            
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text: "Нажми на кнопку, что бы получить возможность отсылать сообщения.",
                replyToMessageId: message.MessageId,
                replyMarkup: captchaMarkup).ConfigureAwait(false);
        }
    }
}