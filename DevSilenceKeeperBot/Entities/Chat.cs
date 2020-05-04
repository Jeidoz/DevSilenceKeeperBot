using System.Collections.Generic;

namespace DevSilenceKeeperBot.Entities
{
    public sealed class Chat
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public List<string> ForbiddenWords { get; set; }
    }
}