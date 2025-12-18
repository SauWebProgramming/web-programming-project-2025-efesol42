using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BendenSana.Migrations
{
    /// <inheritdoc />
    public partial class takasSistemiGuncellendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Status",
                table: "Products");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Status",
                table: "Products",
                sql: "Status IN ('available', 'draft','published','sold','blocked')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Status",
                table: "Products");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Status",
                table: "Products",
                sql: "Status IN ('draft','published','sold','blocked')");
        }
    }
}
