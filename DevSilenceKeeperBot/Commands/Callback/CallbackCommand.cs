using DevSilenceKeeperBot.Commands.Callback.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands.Callback
{
    public abstract class CallbackCommand
    {
        public abstract string[] Triggers { get; }
        public abstract Task Execute(CallbackQuery query);

        public virtual bool Contains(CallbackQuery query)
        {
            var queryData = new CallbackQueryData(query.Data);
            return Triggers.Any(trigger => queryData.Trigger == trigger);
        }
    }
}