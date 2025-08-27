using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EORequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sla_rule_WorkflowStepTemplateId",
                table: "sla_rule");

            migrationBuilder.AddColumn<int>(
                name: "EscalationOffsetDays",
                table: "sla_rule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "sla_rule",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderOffsetsCsv",
                table: "sla_rule",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_sla_rule_WorkflowStepTemplateId",
                table: "sla_rule",
                column: "WorkflowStepTemplateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sla_rule_WorkflowStepTemplateId",
                table: "sla_rule");

            migrationBuilder.DropColumn(
                name: "EscalationOffsetDays",
                table: "sla_rule");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "sla_rule");

            migrationBuilder.DropColumn(
                name: "ReminderOffsetsCsv",
                table: "sla_rule");

            migrationBuilder.CreateIndex(
                name: "IX_sla_rule_WorkflowStepTemplateId",
                table: "sla_rule",
                column: "WorkflowStepTemplateId");
        }
    }
}
