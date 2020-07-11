namespace DevSilenceKeeperBot.Data.Entities
{
    public class ForbiddenChatWord : BaseEntity
    {
        public string Word { get; set; }
        public Chat Chat { get; set; }
    }
}