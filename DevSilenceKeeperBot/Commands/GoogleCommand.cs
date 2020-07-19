using DevSilenceKeeperBot.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class GoogleCommand : Command
    {
        private const int MinWordsGoogleMessageLength = 3;

        public GoogleCommand(string[] triggerWords)
        {
            Triggers = triggerWords;
        }

        public override string[] Triggers { get; }

        public override bool Contains(Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            string formattedText = message.Text.RemoveSpecialCharacters();
            return Triggers.Any(phrase => formattedText.StartsWith(phrase))
                   && formattedText.Split().Length >= MinWordsGoogleMessageLength;
        }

        public override async Task Execute(Message message)
        {
            string imageCaption = $"[{message.Text}](https://www.google.com/search?q={message.Text})\n\n" +
                                  "[Пожалуйста, научитесь гуглить](http://sadykhzadeh.github.io/learn-to-google)";
            await DevSilenceKeeper.BotClient.SendPhotoAsync(
                message.Chat.Id,
                "https://sadykhzadeh.github.io/learn-to-google/img/bad-good.jpg",
                imageCaption,
                ParseMode.MarkdownV2,
                replyToMessageId: message.MessageId,
                disableNotification: true);
        }
    }
}