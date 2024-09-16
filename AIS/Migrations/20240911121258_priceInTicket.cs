using Microsoft.EntityFrameworkCore.Migrations;

namespace AIS.Migrations
{
    public partial class priceInTicket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "Tickets",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Tickets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Tickets",
                newName: "ContactNumber");
        }
    }
}
