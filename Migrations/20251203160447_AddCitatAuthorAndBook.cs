using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBookshelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCitatAuthorAndBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Citats",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Citats",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Revoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citats_BookId",
                table: "Citats",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citats_Books_BookId",
                table: "Citats",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citats_Books_BookId",
                table: "Citats");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Citats_BookId",
                table: "Citats");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Citats");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Citats");
        }
    }
}
