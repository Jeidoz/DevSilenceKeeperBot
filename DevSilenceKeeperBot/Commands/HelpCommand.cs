using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class HelpCommand : Command
    {
        public override string[] Triggers => new[] {"/help"};

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            const string response = "Список команд:\n" +
                                    "/help – показать помощь\n" +
                                    "/words – показать запрещенные строки-шаблоны\n" +
                                    "/templates – альтернатива /words\n" +
                                    "/add – добавить запрещенную строку-шаблон\n" +
                                    "/remove (/rm) – убрать запрещенную строку-шаблон\n" +
                                    "/delete (/del) – удалить сообщение\n" +
                                    "/users (/promoted) – список участников с привилегиями\n" +
                                    "/mute [d.]HH:mm[:ss] - замутить участника на определенный срок\n" +
                                    "/unmute - розмутить участника\n" +
                                    "/promote - наддать привилегии особого участника чата\n" +
                                    "/unpromote - забрать привилегии особого участника чата";
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId).ConfigureAwait(false);
        }
    }
}