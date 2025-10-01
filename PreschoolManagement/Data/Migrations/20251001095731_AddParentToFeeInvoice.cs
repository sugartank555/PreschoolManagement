using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreschoolManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentToFeeInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentId",
                table: "FeeInvoices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeeInvoices_ParentId",
                table: "FeeInvoices",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeeInvoices_AspNetUsers_ParentId",
                table: "FeeInvoices",
                column: "ParentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeeInvoices_AspNetUsers_ParentId",
                table: "FeeInvoices");

            migrationBuilder.DropIndex(
                name: "IX_FeeInvoices_ParentId",
                table: "FeeInvoices");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "FeeInvoices");
        }
    }
}
