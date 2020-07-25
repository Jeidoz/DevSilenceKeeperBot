using System;
using System.Linq;
using System.Text;

namespace DevSilenceKeeperBot.Commands.Callback.Data
{
    public sealed class CallbackQueryData
    {
        public int UserId { get; set; }
        public string Trigger { get; set; }
        public string[] Arguments { get; set; }

        public CallbackQueryData()
        {

        }

        public CallbackQueryData(int userId, string trigger, params string[] arguments)
        {
            UserId = userId;
            Trigger = trigger;
            Arguments = arguments;
        }

        public CallbackQueryData(string callbackStringData)
        {
            // InvokerId:CallbackQueryTrigger:Arg1:Arg2:...
            var queryTemplate = callbackStringData
                .Split(':', StringSplitOptions.RemoveEmptyEntries);
            UserId = Convert.ToInt32(queryTemplate[0]);
            Trigger = queryTemplate[1];
            Arguments = queryTemplate.Skip(2).ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(UserId);
            builder.Append(':');
            builder.Append(Trigger);

            if (Arguments != null)
            {
                foreach (var argument in Arguments)
                {
                    builder.Append(':');
                    builder.Append(argument);
                }
            }

            return builder.ToString();
        }
    }
}
