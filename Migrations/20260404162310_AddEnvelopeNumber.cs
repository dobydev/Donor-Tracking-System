using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvelopeNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnvelopeNumber",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnvelopeNumber",
                table: "Donations");
        }
    }
}
