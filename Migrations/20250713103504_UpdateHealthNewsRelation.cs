using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopDongY.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHealthNewsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
