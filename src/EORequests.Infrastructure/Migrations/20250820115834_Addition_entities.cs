using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EORequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addition_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "request_type",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreparedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_request_request_type_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "request_type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_template",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_template", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_template_request_type_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "request_type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment_thread",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LinkedEntityType = table.Column<int>(type: "int", nullable: false),
                    LinkedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_thread", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comment_thread_request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "request",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "workflow_step_template",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    WorkflowTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AssignmentMode = table.Column<int>(type: "int", nullable: false),
                    AllowedRolesCsv = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowCreatorOrPreparer = table.Column<bool>(type: "bit", nullable: false),
                    BranchRuleKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    JsonSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_step_template", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_step_template_workflow_template_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "workflow_template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comment_comment_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comment_comment_thread_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "comment_thread",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "escalation_rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    WorkflowStepTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalateAfterDays = table.Column<int>(type: "int", nullable: false),
                    EscalateToRolesCsv = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WorkflowTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escalation_rule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_escalation_rule_workflow_step_template_WorkflowStepTemplateId",
                        column: x => x.WorkflowStepTemplateId,
                        principalTable: "workflow_step_template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_escalation_rule_workflow_template_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "workflow_template",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "sla_rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    WorkflowStepTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDays = table.Column<int>(type: "int", nullable: false),
                    WorkflowTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sla_rule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sla_rule_workflow_step_template_WorkflowStepTemplateId",
                        column: x => x.WorkflowStepTemplateId,
                        principalTable: "workflow_step_template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sla_rule_workflow_template_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "workflow_template",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "comment_reaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_reaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comment_reaction_comment_CommentId",
                        column: x => x.CommentId,
                        principalTable: "comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mention",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentionedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mention_comment_CommentId",
                        column: x => x.CommentId,
                        principalTable: "comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LinkedEntityType = table.Column<int>(type: "int", nullable: false),
                    LinkedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SoftDeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkflowStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attachment_request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "request",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "task_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    WorkflowStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsGating = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_item_task_item_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "task_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_instance_request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StateCode = table.Column<int>(type: "int", nullable: false),
                    AssigneeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_state", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_state_workflow_instance_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "workflow_instance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_state_workflow_step_template_StepTemplateId",
                        column: x => x.StepTemplateId,
                        principalTable: "workflow_step_template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachment_LinkedEntityType_LinkedEntityId",
                table: "attachment",
                columns: new[] { "LinkedEntityType", "LinkedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_attachment_RequestId",
                table: "attachment",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_attachment_TaskItemId",
                table: "attachment",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_attachment_VersionGroupId",
                table: "attachment",
                column: "VersionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_attachment_WorkflowStateId",
                table: "attachment",
                column: "WorkflowStateId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_ParentCommentId",
                table: "comment",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_ThreadId",
                table: "comment",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_reaction_CommentId_UserId_Emoji",
                table: "comment_reaction",
                columns: new[] { "CommentId", "UserId", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comment_thread_LinkedEntityType_LinkedEntityId",
                table: "comment_thread",
                columns: new[] { "LinkedEntityType", "LinkedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_comment_thread_RequestId",
                table: "comment_thread",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_escalation_rule_WorkflowStepTemplateId",
                table: "escalation_rule",
                column: "WorkflowStepTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_escalation_rule_WorkflowTemplateId",
                table: "escalation_rule",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_mention_CommentId_MentionedUserId",
                table: "mention",
                columns: new[] { "CommentId", "MentionedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_request_RequestTypeId",
                table: "request",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_request_type_Code",
                table: "request_type",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sla_rule_WorkflowStepTemplateId",
                table: "sla_rule",
                column: "WorkflowStepTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_sla_rule_WorkflowTemplateId",
                table: "sla_rule",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_task_item_ParentTaskId",
                table: "task_item",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_item_WorkflowStateId_Status",
                table: "task_item",
                columns: new[] { "WorkflowStateId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_RequestId",
                table: "workflow_instance",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_state_StepTemplateId",
                table: "workflow_state",
                column: "StepTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_state_WorkflowInstanceId_StepTemplateId",
                table: "workflow_state",
                columns: new[] { "WorkflowInstanceId", "StepTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_step_template_WorkflowTemplateId_StepOrder",
                table: "workflow_step_template",
                columns: new[] { "WorkflowTemplateId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_template_RequestTypeId_Code",
                table: "workflow_template",
                columns: new[] { "RequestTypeId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_attachment_task_item_TaskItemId",
                table: "attachment",
                column: "TaskItemId",
                principalTable: "task_item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_attachment_workflow_state_WorkflowStateId",
                table: "attachment",
                column: "WorkflowStateId",
                principalTable: "workflow_state",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_task_item_workflow_state_WorkflowStateId",
                table: "task_item",
                column: "WorkflowStateId",
                principalTable: "workflow_state",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance",
                column: "CurrentStepId",
                principalTable: "workflow_state",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workflow_instance_request_RequestId",
                table: "workflow_instance");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_instance_workflow_state_CurrentStepId",
                table: "workflow_instance");

            migrationBuilder.DropTable(
                name: "attachment");

            migrationBuilder.DropTable(
                name: "comment_reaction");

            migrationBuilder.DropTable(
                name: "escalation_rule");

            migrationBuilder.DropTable(
                name: "mention");

            migrationBuilder.DropTable(
                name: "sla_rule");

            migrationBuilder.DropTable(
                name: "task_item");

            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "comment_thread");

            migrationBuilder.DropTable(
                name: "request");

            migrationBuilder.DropTable(
                name: "workflow_state");

            migrationBuilder.DropTable(
                name: "workflow_instance");

            migrationBuilder.DropTable(
                name: "workflow_step_template");

            migrationBuilder.DropTable(
                name: "workflow_template");

            migrationBuilder.DropTable(
                name: "request_type");
        }
    }
}
