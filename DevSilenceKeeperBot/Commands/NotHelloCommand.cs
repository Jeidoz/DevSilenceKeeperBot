using DevSilenceKeeperBot.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class NotHelloCommand : Command
    {
        private const int MaxWordsInHelloMesssage = 2;

        public override string[] Triggers { get; }

        public NotHelloCommand(string[] triggerWords)
        {
            Triggers = triggerWords;
        }

        public override bool Contains(Message message)
        {
            string[] words = message.Text.RemoveSpecialCharacters().Split();
            return Triggers.Any(word => words.Contains(word))
                && words.Length <= MaxWordsInHelloMesssage;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: "https://neprivet.ru/img/bad-good.png",
                caption: "[Не привет](https://neprivet.ru)",
                parseMode: ParseMode.MarkdownV2,
                replyToMessageId: message.MessageId,
                disableNotification: true).ConfigureAwait(false);
        }
    }
}