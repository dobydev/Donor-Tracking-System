using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNonMemberLinkToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NonMemberID",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_NonMemberID",
                table: "Donations",
                column: "NonMemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_NonMembers_NonMemberID",
                table: "Donations",
                column: "NonMemberID",
                principalTable: "NonMembers",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_NonMembers_NonMemberID",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_NonMemberID",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "NonMemberID",
                table: "Donations");
        }
    }
}
