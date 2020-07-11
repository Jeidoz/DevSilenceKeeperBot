using System.Collections.Generic;
using DevSilenceKeeperBot.Entities.ManyToMany;

namespace DevSilenceKeeperBot.Entities
{
    public sealed class Chat : BaseEntity
    {
        public long ChatId { get; set; }
        public ICollection<ChatToForbiddenChatWord> ForbiddenWords { get; set; }
        public ICollection<ChatToPromotedMember> PromotedMembers { get; set; }
    }
}