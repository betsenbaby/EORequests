using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EORequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Make_CurrentStepId_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance");

            migrationBuilder.DropIndex(
                name: "IX_workflow_instance_CurrentStepId",
                table: "workflow_instance");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentStepId",
                table: "workflow_instance",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId",
                principalTable: "workflow_state",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance");

            migrationBuilder.DropIndex(
                name: "IX_workflow_instance_CurrentStepId",
                table: "workflow_instance");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentStepId",
                table: "workflow_instance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId",
                principalTable: "workflow_state",
                principalColumn: "Id");
        }
    }
}
