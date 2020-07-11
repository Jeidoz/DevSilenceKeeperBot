namespace DevSilenceKeeperBot.DAL.Entities
{
    public class ForbiddenChatWord : BaseEntity
    {
        public string Word { get; set; }
        public Chat Chat { get; set; }
    }
}