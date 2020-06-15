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
            string response = string.Format(
                "Список команд:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}",
                "/help – показать помощь",
                "/words – показать запрещенные строки-шаблоны",
                "/templates – альтернатива /words",
                "/add – добавить запрещенную строку-шаблон",
                "/remove (/rm) – убрать запрещенную строку-шаблон",
                "/delete (/del) – альтернатива /remove",
                "/users (/promoted) – список участников с привилегиями",
                "/mute dd.hh:mm:ss - замутить участника на определенный срок",
                "/unmute - розмутить участника",
                "/promote - наддать привилегии особого участника чата",
                "/unpromote - забрать привилегии особого участника чата");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                replyToMessageId: message.MessageId).ConfigureAwait(false);
        }
    }
}