using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackCubed.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeletionProblem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CubedItemTag_CubedItems_CubedItemsId",
                table: "CubedItemTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_ApplicationUsers_UserId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_CubedItemTag_CubedItems_CubedItemsId",
                table: "CubedItemTag",
                column: "CubedItemsId",
                principalTable: "CubedItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_ApplicationUsers_UserId",
                table: "Tags",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CubedItemTag_CubedItems_CubedItemsId",
                table: "CubedItemTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_ApplicationUsers_UserId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_CubedItemTag_CubedItems_CubedItemsId",
                table: "CubedItemTag",
                column: "CubedItemsId",
                principalTable: "CubedItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_ApplicationUsers_UserId",
                table: "Tags",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");
        }
    }
}
