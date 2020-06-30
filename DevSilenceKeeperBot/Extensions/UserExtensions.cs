using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Extensions
{
    public static class UserExtensions
    {
        public static async Task<bool> IsAdmin(this User sender, long chatId)
        {
            var chatMemberDetails = await DevSilenceKeeper.BotClient.GetChatMemberAsync(chatId, sender.Id);
            return chatMemberDetails.Status == ChatMemberStatus.Administrator
                   || chatMemberDetails.Status == ChatMemberStatus.Creator;
        }

        public static async Task<bool> IsMuted(this User sender, long chatId)
        {
            var chatMemberDetails = await DevSilenceKeeper.BotClient.GetChatMemberAsync(chatId, sender.Id);
            return chatMemberDetails.UntilDate != null && chatMemberDetails.UntilDate > DateTime.Now.ToUniversalTime();
        }
    }
}