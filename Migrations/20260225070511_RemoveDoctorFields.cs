using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLife.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDoctorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorContact",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "DoctorContact",
                table: "DonationRequests");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "DonationRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorContact",
                table: "Recipients",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Recipients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorContact",
                table: "DonationRequests",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "DonationRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
