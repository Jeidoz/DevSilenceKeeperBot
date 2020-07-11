using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DevSilenceKeeperBot.Migrations
{
    public partial class DbInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChatId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotedMembers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotedMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForbiddenChatWords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Word = table.Column<string>(nullable: true),
                    ChatId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForbiddenChatWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForbiddenChatWords_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatToPromotedMembers",
                columns: table => new
                {
                    ChatId = table.Column<int>(nullable: false),
                    PromotedMemberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatToPromotedMembers", x => new { x.ChatId, x.PromotedMemberId });
                    table.ForeignKey(
                        name: "FK_ChatToPromotedMembers_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatToPromotedMembers_PromotedMembers_PromotedMemberId",
                        column: x => x.PromotedMemberId,
                        principalTable: "PromotedMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatToPromotedMembers_PromotedMemberId",
                table: "ChatToPromotedMembers",
                column: "PromotedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ForbiddenChatWords_ChatId",
                table: "ForbiddenChatWords",
                column: "ChatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatToPromotedMembers");

            migrationBuilder.DropTable(
                name: "ForbiddenChatWords");

            migrationBuilder.DropTable(
                name: "PromotedMembers");

            migrationBuilder.DropTable(
                name: "Chats");
        }
    }
}
