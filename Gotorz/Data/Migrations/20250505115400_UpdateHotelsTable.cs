using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gotorz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHotelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightInfos_TravelPackages_TravelPackageId",
                table: "FlightInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_HotelInfos_TravelPackages_TravelPackageId",
                table: "HotelInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HotelInfos",
                table: "HotelInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FlightInfos",
                table: "FlightInfos");

            migrationBuilder.RenameTable(
                name: "HotelInfos",
                newName: "HotelInfo");

            migrationBuilder.RenameTable(
                name: "FlightInfos",
                newName: "FlightInfo");

            migrationBuilder.RenameIndex(
                name: "IX_HotelInfos_TravelPackageId",
                table: "HotelInfo",
                newName: "IX_HotelInfo_TravelPackageId");

            migrationBuilder.RenameIndex(
                name: "IX_FlightInfos_TravelPackageId",
                table: "FlightInfo",
                newName: "IX_FlightInfo_TravelPackageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HotelInfo",
                table: "HotelInfo",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FlightInfo",
                table: "FlightInfo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightInfo_TravelPackages_TravelPackageId",
                table: "FlightInfo",
                column: "TravelPackageId",
                principalTable: "TravelPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HotelInfo_TravelPackages_TravelPackageId",
                table: "HotelInfo",
                column: "TravelPackageId",
                principalTable: "TravelPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightInfo_TravelPackages_TravelPackageId",
                table: "FlightInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_HotelInfo_TravelPackages_TravelPackageId",
                table: "HotelInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HotelInfo",
                table: "HotelInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FlightInfo",
                table: "FlightInfo");

            migrationBuilder.RenameTable(
                name: "HotelInfo",
                newName: "HotelInfos");

            migrationBuilder.RenameTable(
                name: "FlightInfo",
                newName: "FlightInfos");

            migrationBuilder.RenameIndex(
                name: "IX_HotelInfo_TravelPackageId",
                table: "HotelInfos",
                newName: "IX_HotelInfos_TravelPackageId");

            migrationBuilder.RenameIndex(
                name: "IX_FlightInfo_TravelPackageId",
                table: "FlightInfos",
                newName: "IX_FlightInfos_TravelPackageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HotelInfos",
                table: "HotelInfos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FlightInfos",
                table: "FlightInfos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightInfos_TravelPackages_TravelPackageId",
                table: "FlightInfos",
                column: "TravelPackageId",
                principalTable: "TravelPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HotelInfos_TravelPackages_TravelPackageId",
                table: "HotelInfos",
                column: "TravelPackageId",
                principalTable: "TravelPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
