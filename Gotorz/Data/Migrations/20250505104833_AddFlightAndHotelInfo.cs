using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gotorz.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightAndHotelInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelPackages_Bookings_BookingId",
                table: "TravelPackages");

            migrationBuilder.DropIndex(
                name: "IX_TravelPackages_BookingId",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "TravelPackages");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "TravelPackages",
                type: "int",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Adress",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Adults",
                table: "TravelPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Airline",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalAirport",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "TravelPackages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "TravelPackages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FlightArrivalTime",
                table: "TravelPackages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FlightDepartureTime",
                table: "TravelPackages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FlightInfo",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HotelAddress",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HotelName",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kids",
                table: "TravelPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualPrice",
                table: "TravelPackages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginAirport",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PriceMarkupPercentage",
                table: "TravelPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "TravelPackages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ReturnFlightIncluded",
                table: "TravelPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Rooms",
                table: "TravelPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TotalPrice",
                table: "Bookings",
                type: "int",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "BookingTravelPackage",
                columns: table => new
                {
                    BookingsId = table.Column<int>(type: "int", nullable: false),
                    TravelPackagesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTravelPackage", x => new { x.BookingsId, x.TravelPackagesId });
                    table.ForeignKey(
                        name: "FK_BookingTravelPackage_Bookings_BookingsId",
                        column: x => x.BookingsId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingTravelPackage_TravelPackages_TravelPackagesId",
                        column: x => x.TravelPackagesId,
                        principalTable: "TravelPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiHotelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromPrice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StarRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HotelRoomTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelRoomTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelRoomTypes_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingTravelPackage_TravelPackagesId",
                table: "BookingTravelPackage",
                column: "TravelPackagesId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelRoomTypes_HotelId",
                table: "HotelRoomTypes",
                column: "HotelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingTravelPackage");

            migrationBuilder.DropTable(
                name: "HotelRoomTypes");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropColumn(
                name: "Adress",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "Adults",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "Airline",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "ArrivalAirport",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "FlightArrivalTime",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "FlightDepartureTime",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "FlightInfo",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "HotelAddress",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "HotelName",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "Kids",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "ManualPrice",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "OriginAirport",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "PriceMarkupPercentage",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "ReturnFlightIncluded",
                table: "TravelPackages");

            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "TravelPackages");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "TravelPackages",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TravelPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "TravelPackages",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_BookingId",
                table: "TravelPackages",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelPackages_Bookings_BookingId",
                table: "TravelPackages",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");
        }
    }
}
