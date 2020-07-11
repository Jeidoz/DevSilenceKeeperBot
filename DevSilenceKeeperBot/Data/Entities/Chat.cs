using System.Collections.Generic;
using DevSilenceKeeperBot.DAL.Entities.ManyToMany;

namespace DevSilenceKeeperBot.DAL.Entities
{
    public sealed class Chat : BaseEntity
    {
        public long ChatId { get; set; }
        public ICollection<ForbiddenChatWord> ForbiddenWords { get; set; }
        public ICollection<ChatToPromotedMember> PromotedMembers { get; set; }
    }
}