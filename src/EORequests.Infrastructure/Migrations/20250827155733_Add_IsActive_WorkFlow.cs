using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EORequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsActive_WorkFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "workflow_template",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "workflow_template");
        }
    }
}
