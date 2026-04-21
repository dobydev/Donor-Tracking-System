using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonorTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FamilyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FamilyID",
                table: "Congregants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Congregants_FamilyID",
                table: "Congregants",
                column: "FamilyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Congregants_Families_FamilyID",
                table: "Congregants",
                column: "FamilyID",
                principalTable: "Families",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Congregants_Families_FamilyID",
                table: "Congregants");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Congregants_FamilyID",
                table: "Congregants");

            migrationBuilder.DropColumn(
                name: "FamilyID",
                table: "Congregants");
        }
    }
}
