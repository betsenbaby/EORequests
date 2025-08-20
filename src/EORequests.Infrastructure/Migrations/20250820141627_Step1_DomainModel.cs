using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EORequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Step1_DomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndexNumber",
                table: "application_user",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "application_user",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexNumber",
                table: "application_user");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "application_user");
        }
    }
}
