using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCongregantLinkToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CongregantID",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CongregantID",
                table: "Donations",
                column: "CongregantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Congregants_CongregantID",
                table: "Donations",
                column: "CongregantID",
                principalTable: "Congregants",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Congregants_CongregantID",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_CongregantID",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "CongregantID",
                table: "Donations");
        }
    }
}
