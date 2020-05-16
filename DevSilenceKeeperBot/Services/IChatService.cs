using DevSilenceKeeperBot.Entities;
using System.Collections.Generic;

namespace DevSilenceKeeperBot.Services
{
    public interface IChatService
    {
        IEnumerable<string> GetChatForbiddenWords(long chatId);
        Chat AddChatForbiddenWord(long chatId, string word);
        bool RemoveChatForbiddenWord(long chatId, string word);
    }
}