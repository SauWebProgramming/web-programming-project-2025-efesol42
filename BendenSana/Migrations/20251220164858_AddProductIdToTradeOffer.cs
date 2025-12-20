using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BendenSana.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToTradeOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Trade_Offers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Trade_Offers");
        }
    }
}
