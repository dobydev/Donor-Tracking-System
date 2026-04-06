using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class NonCongregant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_NonMembers_NonMemberID",
                table: "Donations");

            migrationBuilder.DropTable(
                name: "NonMembers");

            migrationBuilder.RenameColumn(
                name: "NonMemberID",
                table: "Donations",
                newName: "NonCongregantID");

            migrationBuilder.RenameIndex(
                name: "IX_Donations_NonMemberID",
                table: "Donations",
                newName: "IX_Donations_NonCongregantID");

            migrationBuilder.CreateTable(
                name: "NonCongregants",
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
                    table.PrimaryKey("PK_NonCongregants", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_NonCongregants_NonCongregantID",
                table: "Donations",
                column: "NonCongregantID",
                principalTable: "NonCongregants",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_NonCongregants_NonCongregantID",
                table: "Donations");

            migrationBuilder.DropTable(
                name: "NonCongregants");

            migrationBuilder.RenameColumn(
                name: "NonCongregantID",
                table: "Donations",
                newName: "NonMemberID");

            migrationBuilder.RenameIndex(
                name: "IX_Donations_NonCongregantID",
                table: "Donations",
                newName: "IX_Donations_NonMemberID");

            migrationBuilder.CreateTable(
                name: "NonMembers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyOrganization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonMembers", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_NonMembers_NonMemberID",
                table: "Donations",
                column: "NonMemberID",
                principalTable: "NonMembers",
                principalColumn: "ID");
        }
    }
}
