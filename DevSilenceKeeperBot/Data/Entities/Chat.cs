using System.Collections.Generic;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;

namespace DevSilenceKeeperBot.Data.Entities
{
    public sealed class Chat : BaseEntity
    {
        public long ChatId { get; set; }
        public IList<ForbiddenChatWord> ForbiddenWords { get; set; }
        public IList<ChatToPromotedMember> PromotedMembers { get; set; }
        
        public Chat(long chatId)
        {
            ChatId = chatId;
            ForbiddenWords = new List<ForbiddenChatWord>();
            PromotedMembers = new List<ChatToPromotedMember>();
        }
    }
}