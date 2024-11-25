using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateUsersAndRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    BlueskyDid = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FishyFlipSessionEncrypted = table.Column<string>(type: "character varying(20480)", maxLength: 20480, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.BlueskyDid);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UserBlueskyDid = table.Column<string>(type: "character varying(1024)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserBlueskyDid",
                        column: x => x.UserBlueskyDid,
                        principalTable: "Users",
                        principalColumn: "BlueskyDid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserBlueskyDid",
                table: "RefreshTokens",
                column: "UserBlueskyDid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
