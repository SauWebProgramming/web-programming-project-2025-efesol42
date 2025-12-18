using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BendenSana.Migrations
{
    /// <inheritdoc />
    public partial class FixStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TradeOffers_Status",
                table: "Trade_Offers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TradeOffers_Status",
                table: "Trade_Offers",
                sql: "Status IN ('pending', 'accepted', 'rejected', 'cancelled', 'Pending', 'Accepted', 'Rejected', 'Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TradeOffers_Status",
                table: "Trade_Offers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TradeOffers_Status",
                table: "Trade_Offers",
                sql: "Status IN ('pending','accepted','rejected','cancelled')");
        }
    }
}
