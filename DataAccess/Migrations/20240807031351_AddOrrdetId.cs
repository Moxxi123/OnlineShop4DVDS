using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrrdetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "Reviewes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviewes_OrderItemId",
                table: "Reviewes",
                column: "OrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviewes_OrderItems_OrderItemId",
                table: "Reviewes",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviewes_OrderItems_OrderItemId",
                table: "Reviewes");

            migrationBuilder.DropIndex(
                name: "IX_Reviewes_OrderItemId",
                table: "Reviewes");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Reviewes");
        }
    }
}
