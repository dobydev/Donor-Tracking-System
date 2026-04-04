using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Committees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Committees", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Congregants",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveStatus = table.Column<int>(type: "int", nullable: false),
                    CommitteeMemberships = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Congregants", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FundDesignations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveStatus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundDesignations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NonMembers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyOrganization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonMembers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonorID = table.Column<int>(type: "int", nullable: false),
                    DonationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FundDesignationID = table.Column<int>(type: "int", nullable: false),
                    StaffMemberID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Donations_FundDesignations_FundDesignationID",
                        column: x => x.FundDesignationID,
                        principalTable: "FundDesignations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FundDesignations",
                columns: new[] { "ID", "ActiveStatus", "Name" },
                values: new object[,]
                {
                    { 1, true, "General Fund" },
                    { 2, true, "Building Fund" },
                    { 3, true, "Missions" },
                    { 4, true, "Benevolence" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_FundDesignationID",
                table: "Donations",
                column: "FundDesignationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Committees");

            migrationBuilder.DropTable(
                name: "Congregants");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "NonMembers");

            migrationBuilder.DropTable(
                name: "FundDesignations");
        }
    }
}
