using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIS.Migrations
{
    public partial class tickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Flights_FlightId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "FlightRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket");

            migrationBuilder.RenameTable(
                name: "Ticket",
                newName: "Tickets");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_FlightId",
                table: "Tickets",
                newName: "IX_Tickets_FlightId");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Seat",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Flights_FlightId",
                table: "Tickets",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Flights_FlightId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Seat",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tickets");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Ticket");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_FlightId",
                table: "Ticket",
                newName: "IX_Ticket_FlightId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FlightRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    Arrival = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Departure = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginId = table.Column<int>(type: "int", nullable: false),
                    PassengerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightRecords", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Flights_FlightId",
                table: "Ticket",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
