using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackCubed.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLInkPreviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewDescription",
                table: "CubedItems",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "CubedItems",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewTitle",
                table: "CubedItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewDescription",
                table: "CubedItems");

            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "CubedItems");

            migrationBuilder.DropColumn(
                name: "PreviewTitle",
                table: "CubedItems");
        }
    }
}
