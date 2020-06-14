using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class HelpCommand : Command
    {
        public override string[] Triggers => new string[] { "/help" };

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            string response = string.Format("Список комманд:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
                        "/help – показать помощь",
                        "/words – показать запрещенные строки-шаблоны",
                        "/templates – альтернатива /words",
                        "/add – добавить запрещенную строку-шаблон",
                        "/remove (/rm) – убрать запрещенную строку-шаблон",
                        "/delete (/del) – альтернатива /remove");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                replyToMessageId: message.MessageId);
        }
    }
}