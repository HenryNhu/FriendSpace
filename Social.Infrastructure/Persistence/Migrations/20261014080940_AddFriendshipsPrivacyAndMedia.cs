using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendshipsPrivacyAndMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "friendships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserLowId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserHighId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friendships", x => x.Id);
                    table.CheckConstraint("ck_friendships_pair", "\"UserLowId\" < \"UserHighId\"");
                    table.CheckConstraint("ck_friendships_requester", "\"RequestedById\" IN (\"UserLowId\", \"UserHighId\")");
                    table.ForeignKey(
                        name: "FK_friendships_AspNetUsers_UserHighId",
                        column: x => x.UserHighId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_friendships_AspNetUsers_UserLowId",
                        column: x => x.UserLowId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAvatar = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_assets_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_assets_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_posts_AuthorId_CreatedAt_Id",
                table: "posts",
                columns: new[] { "AuthorId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_friendships_UserHighId_Status",
                table: "friendships",
                columns: new[] { "UserHighId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_friendships_UserLowId_UserHighId",
                table: "friendships",
                columns: new[] { "UserLowId", "UserHighId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_BlobName",
                table: "media_assets",
                column: "BlobName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_CreatedAt",
                table: "media_assets",
                column: "CreatedAt",
                filter: "\"PostId\" IS NULL AND \"IsAvatar\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_OwnerId",
                table: "media_assets",
                column: "OwnerId",
                unique: true,
                filter: "\"IsAvatar\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_PostId",
                table: "media_assets",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "friendships");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropIndex(
                name: "IX_posts_AuthorId_CreatedAt_Id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "posts");
        }
    }
}
