using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackCubed.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemTypeNameFromCubedItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemTypeName",
                table: "CubedItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemTypeName",
                table: "CubedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
