using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matchday.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Fields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Fields",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
