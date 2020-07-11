using Microsoft.EntityFrameworkCore;

namespace DevSilenceKeeperBot.DAL
{
    public sealed class BotDbContext : DbContext
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<PromotedMember> PromotedMembers { get; set; }
        
        #region Many-To-Many table accestors
        
        public DbSet<ChatToPromotedMember> ChatPromotedMembers { get; set; }
        public DbSet<ChatToForbiddenChatWord> ChatForbiddenChatWords { get; set; }
        
        #endregion
        
        public BotDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
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
                .HasOne(cpm => cpm.PromotedMember)
                .WithMany(cpm => cpm.Chats)
                .HasForeignKey(cpm => cpm.PromotedMemberId);
            
            #endregion

            #region Many-To-Many Chats-ForbiddenChatWords

            modelBuilder.Entity<ChatToForbiddenChatWord>()
                .HasKey(cfw => new {cfw.ChatId, cfw.ForbiddenChatWordId});

            modelBuilder.Entity<ChatToForbiddenChatWord>()
                .HasOne(cfw => cfw.Chat)
                .WithMany(cfw => cfw.ForbiddenWords)
                .HasForeignKey(cfw => cfw.ChatId);

            modelBuilder.Entity<ChatToForbiddenChatWord>()
                .HasOne(cfw => cfw.ForbiddenChatWord)
                .WithMany(cfw => cfw.Chats)
                .HasForeignKey(cfw => cfw.ForbiddenChatWordId);

            #endregion
        }
    }
}