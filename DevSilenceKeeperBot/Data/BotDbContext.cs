using DevSilenceKeeperBot.Data.Entities;
using DevSilenceKeeperBot.Data.Entities.ManyToMany;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DevSilenceKeeperBot.Data
{
    public sealed class BotDbContext : DbContext
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<PromotedChatMember> PromotedMembers { get; set; }
        public DbSet<ForbiddenChatWord> ForbiddenChatWords { get; set; }
        
        #region Many-To-Many table accestors
        
        public DbSet<ChatToPromotedMember> ChatToPromotedMembers { get; set; }

        #endregion

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Many-To-Many Chats-PromotedMembers

            modelBuilder.Entity<ChatToPromotedMember>()
                .HasKey(cpt => new {cpt.ChatId, cpt.PromotedMemberId});

            modelBuilder.Entity<ChatToPromotedMember>()
                .HasOne(cpm => cpm.Chat)
                .WithMany(cpm => cpm.PromotedMembers)
                .HasForeignKey(cpm => cpm.ChatId);
            modelBuilder.Entity<ChatToPromotedMember>()
                .HasOne(cpm => cpm.PromotedChatMember)
                .WithMany(cpm => cpm.Chats)
                .HasForeignKey(cpm => cpm.PromotedMemberId);
            
            #endregion
        }
    }
}