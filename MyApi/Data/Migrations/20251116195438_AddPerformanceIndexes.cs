using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Receipts_Merchant",
                table: "Receipts",
                column: "Merchant");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PurchaseDate",
                table: "Receipts",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_UserId_UploadedAt",
                table: "Receipts",
                columns: new[] { "UserId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_UserId_WarrantyExpirationDate",
                table: "Receipts",
                columns: new[] { "UserId", "WarrantyExpirationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_WarrantyExpirationDate",
                table: "Receipts",
                column: "WarrantyExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receipts_Merchant",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_PurchaseDate",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_UserId_UploadedAt",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_UserId_WarrantyExpirationDate",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_WarrantyExpirationDate",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_UserId_CreatedAt",
                table: "ChatMessages");
        }
    }
}
