using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ListCommand : Command
    {
        private readonly IChatService _chatService;

        public ListCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/words", "/templates", "/list"};

        public override async Task Execute(Message message)
        {
            var chatForbiddenWords = _chatService
                .GetChatForbiddenWords(message.Chat.Id)
                .ToArray();
            string response;
            if (chatForbiddenWords.Length > 0)
            {
                string templates = string.Join('\n', chatForbiddenWords);
                response = $"Строки-шаблоны в банлисте:\n{templates}";
            }
            else
            {
                response = "Банлист пуст.";
            }

            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId);
        }
    }
}