using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gotorz.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentReplyToCustomerInquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentReply",
                table: "CustomerInquiries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepliedBy",
                table: "CustomerInquiries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReplyDate",
                table: "CustomerInquiries",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentReply",
                table: "CustomerInquiries");

            migrationBuilder.DropColumn(
                name: "RepliedBy",
                table: "CustomerInquiries");

            migrationBuilder.DropColumn(
                name: "ReplyDate",
                table: "CustomerInquiries");
        }
    }
}
