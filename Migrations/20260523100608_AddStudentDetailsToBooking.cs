using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailGo.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentDetailsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassengerType",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentCardNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "University",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassengerType",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StudentCardNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "University",
                table: "Bookings");
        }
    }
}
