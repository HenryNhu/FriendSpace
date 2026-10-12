using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePostLikesTableAndCreatedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_postlikes",
                table: "postlikes");

            migrationBuilder.RenameTable(
                name: "postlikes",
                newName: "post_likes");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "post_likes",
                newName: "CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_post_likes",
                table: "post_likes",
                columns: new[] { "PostId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_post_likes",
                table: "post_likes");

            migrationBuilder.RenameTable(
                name: "post_likes",
                newName: "postlikes");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "postlikes",
                newName: "CreateAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_postlikes",
                table: "postlikes",
                columns: new[] { "PostId", "UserId" });
        }
    }
}
