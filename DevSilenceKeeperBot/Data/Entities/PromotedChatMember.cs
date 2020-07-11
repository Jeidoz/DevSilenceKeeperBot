using System.Collections.Generic;
using System.Text;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Data.Entities
{
    public sealed class PromotedChatMember : BaseEntity
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }

        public IList<ChatToPromotedMember> Chats { get; set; }

        public PromotedChatMember()
        {
            Chats = new List<ChatToPromotedMember>();
        }

        public PromotedChatMember(User user) : this()
        {
            UserId = user.Id;
            Username = user.Username;
            
            var builder = new StringBuilder(capacity: 256);
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                builder.Append(user.FirstName);
                builder.Append(' ');
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                builder.Append(user.LastName);
            }

            FullName = builder.ToString();
        }
    }
}