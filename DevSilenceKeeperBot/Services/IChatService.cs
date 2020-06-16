using System.Collections.Generic;
using DevSilenceKeeperBot.Entities;
using Telegram.Bot.Types;
using Chat = DevSilenceKeeperBot.Entities.Chat;

namespace DevSilenceKeeperBot.Services
{
    public interface IChatService
    {
        IEnumerable<string> GetChatForbiddenWords(long chatId);

        Chat AddChatForbiddenWord(long chatId, string word);

        bool RemoveChatForbiddenWord(long chatId, string word);

        IEnumerable<PromotedMember> GetPromotedMembers(long chatId);

        Chat AddPromotedMember(long chatId, User chatMember);

        bool RemovePromotedMember(long chatId, int memberId);
    }
}