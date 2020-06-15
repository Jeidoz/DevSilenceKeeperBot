using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Extensions
{
    public static class UserExtensions
    {
        public static async Task<bool> IsAdmin(this User sender, long chatId, TelegramBotClient botClient)
        {
            var chatAdmins = await botClient.GetChatAdministratorsAsync(chatId);
            return chatAdmins.Any(member => member.User.Id == sender.Id);
        }
    }
}