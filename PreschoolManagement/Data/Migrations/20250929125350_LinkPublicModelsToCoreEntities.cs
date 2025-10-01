using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreschoolManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkPublicModelsToCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassRoomId",
                table: "VisitRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentId",
                table: "VisitRegistrations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "VisitRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitSlot",
                table: "VisitRegistrations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "ContactMessages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedStudentId",
                table: "ContactMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitRegistrations_ClassRoomId",
                table: "VisitRegistrations",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitRegistrations_ParentId",
                table: "VisitRegistrations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitRegistrations_StudentId",
                table: "VisitRegistrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedById",
                table: "ContactMessages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_RelatedStudentId",
                table: "ContactMessages",
                column: "RelatedStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_AspNetUsers_CreatedById",
                table: "ContactMessages",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_Students_RelatedStudentId",
                table: "ContactMessages",
                column: "RelatedStudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitRegistrations_AspNetUsers_ParentId",
                table: "VisitRegistrations",
                column: "ParentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitRegistrations_ClassRooms_ClassRoomId",
                table: "VisitRegistrations",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitRegistrations_Students_StudentId",
                table: "VisitRegistrations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_AspNetUsers_CreatedById",
                table: "ContactMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_Students_RelatedStudentId",
                table: "ContactMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitRegistrations_AspNetUsers_ParentId",
                table: "VisitRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitRegistrations_ClassRooms_ClassRoomId",
                table: "VisitRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitRegistrations_Students_StudentId",
                table: "VisitRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_VisitRegistrations_ClassRoomId",
                table: "VisitRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_VisitRegistrations_ParentId",
                table: "VisitRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_VisitRegistrations_StudentId",
                table: "VisitRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_CreatedById",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_RelatedStudentId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "VisitRegistrations");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "VisitRegistrations");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "VisitRegistrations");

            migrationBuilder.DropColumn(
                name: "VisitSlot",
                table: "VisitRegistrations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "RelatedStudentId",
                table: "ContactMessages");
        }
    }
}
