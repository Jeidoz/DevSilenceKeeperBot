using DevSilenceKeeperBot.Services;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ListCommand : Command
    {
        private readonly IChatService _chatService;

        public override string[] Triggers => new string[] { "/words", "/templates", "/list" };

        public ListCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var chatForbiddenWords = _chatService.GetChatForbiddenWords(message.Chat.Id);
            string response;
            if (chatForbiddenWords?.Count() > 0)
            {
                string templates = string.Join('\n', chatForbiddenWords);
                response = $"Cтроки-шаблоны в банлисте:\n{templates}";
            }
            else
            {
                response = $"Банлист пуст.";
            }

            await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: response,
                    replyToMessageId: message.MessageId);
        }
    }
}
