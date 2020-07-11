using System.Collections.Generic;
using DevSilenceKeeperBot.DAL.Entities.ManyToMany;

namespace DevSilenceKeeperBot.DAL.Entities
{
    public sealed class PromotedMember : BaseEntity
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        
        public ICollection<ChatToPromotedMember> Chats { get; set; }
    }
}