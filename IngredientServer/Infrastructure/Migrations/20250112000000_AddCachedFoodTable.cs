using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngredientServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedFoodTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedFoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", "IdentityColumn"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    SearchKey = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    PreparationTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CookingTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Protein = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Carbohydrates = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Fat = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Fiber = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Instructions = table.Column<string>(type: "json", nullable: false),
                    Tips = table.Column<string>(type: "json", nullable: false),
                    Ingredients = table.Column<string>(type: "json", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    HitCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedFoods", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CachedFoods_SearchKey",
                table: "CachedFoods",
                column: "SearchKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CachedFoods_Name",
                table: "CachedFoods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CachedFoods_LastAccessedAt",
                table: "CachedFoods",
                column: "LastAccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CachedFoods_HitCount",
                table: "CachedFoods",
                column: "HitCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedFoods");
        }
    }
}

