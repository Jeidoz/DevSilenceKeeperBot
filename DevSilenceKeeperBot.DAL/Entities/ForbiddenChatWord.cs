using System.Collections.Generic;
using DevSilenceKeeperBot.Entities.ManyToMany;

namespace DevSilenceKeeperBot.Entities
{
    public class ForbiddenChatWord : BaseEntity
    {
        public string Word { get; set; }
        public ICollection<ChatToForbiddenChatWord> Chats { get; set; }
    }
}