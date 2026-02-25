using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLife.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContactPersonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPersonDesignation",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "Hospitals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPersonDesignation",
                table: "Hospitals",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "Hospitals",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
