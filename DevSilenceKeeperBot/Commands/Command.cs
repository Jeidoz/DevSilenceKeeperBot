using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public abstract class Command
    {
        public abstract string[] Triggers { get; }

        public abstract Task Execute(Message message, TelegramBotClient botClient);

        public virtual bool Contains(Message message)
        {
            var isCommandType = message.Entities
                ?.Any(entity => entity.Type == MessageEntityType.BotCommand);
            if (message.Type != MessageType.Text || isCommandType == false)
            {
                return false;
            }

            return Triggers.Any(name => message.Text.StartsWith(name));
        }
    }
}