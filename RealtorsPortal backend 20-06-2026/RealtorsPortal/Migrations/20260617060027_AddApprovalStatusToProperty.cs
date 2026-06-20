using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtorsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStatusToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Properties");
        }
    }
}
