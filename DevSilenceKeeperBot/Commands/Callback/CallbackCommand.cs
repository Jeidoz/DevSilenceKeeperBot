using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands.Callback
{
    public abstract class CallbackCommand
    {
        protected int InvokerId { get; set; }
        protected string[] Arguments { get; set; }
        public abstract string[] Triggers { get; }
        public abstract Task Execute(CallbackQuery query, TelegramBotClient botClient);
        public virtual bool Contains(CallbackQuery query)
        {
            // InvokerId:CallbackQueryTrigger:Arg1:Arg2:...
            var queryTemplate = query.Data
                .Split(':', StringSplitOptions.RemoveEmptyEntries);
            InvokerId = Convert.ToInt32(queryTemplate[0]);
            Arguments = queryTemplate.Skip(2).ToArray();
            return Triggers.Any(name => queryTemplate[1] == name);
        }
    }
}