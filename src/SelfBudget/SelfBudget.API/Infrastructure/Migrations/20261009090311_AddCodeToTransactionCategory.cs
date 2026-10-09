using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfBudget.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToTransactionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "transaction_categories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "transaction_categories");
        }
    }
}
