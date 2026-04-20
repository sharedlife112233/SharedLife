using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLife.Migrations
{
    /// <inheritdoc />
    public partial class AddWillingToDonateBloodPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WillingToDonateBlood",
                table: "Donors",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WillingToDonateBlood",
                table: "Donors");
        }
    }
}
