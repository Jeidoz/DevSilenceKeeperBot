﻿using DevSilenceKeeperBot.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class NotHelloCommand : Command
    {
        private const int MaxWordsInHelloMessage = 2;

        public NotHelloCommand(string[] triggerWords)
        {
            Triggers = triggerWords;
        }

        public override string[] Triggers { get; }

        public override bool Contains(Message message)
        {
            if (string.IsNullOrEmpty(message.Text) || Triggers is null)
            {
                return false;
            }

            // Ignore forwarded messages
            if (message.ForwardFrom != null)
            {
                return false;
            }

            var words = message.Text.RemoveSpecialCharacters().Split();
            return Triggers.Any(word => words.Contains(word))
                   && words.Length <= MaxWordsInHelloMessage;
        }

        public override async Task Execute(Message message)
        {
            await DevSilenceKeeper.BotClient.SendPhotoAsync(
                message.Chat.Id,
                "https://neprivet.ru/img/bad-good.png",
                "[Не привет](https://neprivet.ru)",
                ParseMode.MarkdownV2,
                replyToMessageId: message.MessageId,
                disableNotification: true);
        }
    }
}