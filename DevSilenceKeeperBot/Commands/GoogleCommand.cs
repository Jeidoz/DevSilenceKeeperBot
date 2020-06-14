using DevSilenceKeeperBot.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class GoogleCommand : Command
    {
        private const int MinWordsGoogleMesssageLength = 3;

        public override string[] Triggers { get; }

        public GoogleCommand(string[] triggerWords)
        {
            Triggers = triggerWords;
        }

        public override bool Contains(Message message)
        {
            string formattedText = message.Text.RemoveSpecialCharacters();
            return Triggers.Any(phrase => formattedText.StartsWith(phrase))
                && formattedText.Split().Length >= MinWordsGoogleMesssageLength;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            string imageCaption = $"[{message.Text}](https://www.google.com/search?q={message.Text})\n\n" +
                    "[Пожалуйста, научитесь гуглить](http://sadykhzadeh.github.io/learn-to-google)";
            await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: "https://sadykhzadeh.github.io/learn-to-google/img/bad-good.jpg",
                caption: imageCaption,
                parseMode: ParseMode.MarkdownV2,
                replyToMessageId: message.MessageId,
                disableNotification: true);
        }
    }
}
