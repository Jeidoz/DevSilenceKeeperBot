namespace DevSilenceKeeperBot.DAL.Entities.ManyToMany
{
    public class ChatToPromotedMember
    {
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
        
        public int PromotedMemberId { get; set; }
        public PromotedChatMember PromotedChatMember { get; set; }
    }
}