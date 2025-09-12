using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackCubed.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreatedCubedItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CubedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SourceFileUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLastAccessed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CubedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CubedItems_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CubedItemTag",
                columns: table => new
                {
                    CubedItemsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CubedItemTag", x => new { x.CubedItemsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_CubedItemTag_CubedItems_CubedItemsId",
                        column: x => x.CubedItemsId,
                        principalTable: "CubedItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CubedItemTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CubedItems_CreatedById",
                table: "CubedItems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CubedItemTag_TagsId",
                table: "CubedItemTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CubedItemTag");

            migrationBuilder.DropTable(
                name: "CubedItems");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
