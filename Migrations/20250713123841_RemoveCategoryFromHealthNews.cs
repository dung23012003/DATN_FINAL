﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopDongY.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryFromHealthNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews");

            migrationBuilder.DropIndex(
                name: "IX_HealthNews_CategoryId",
                table: "HealthNews");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "HealthNews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "HealthNews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HealthNews_CategoryId",
                table: "HealthNews",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthNews_Categories_CategoryId",
                table: "HealthNews",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
