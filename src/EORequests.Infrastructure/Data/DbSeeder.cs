// Infrastructure/Seed/DbSeeder.cs
using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EORequests.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(EoDbContext db)
    {
        if (!await db.RequestTypes.AnyAsync())
        {
            var cr = new RequestType { Code = "CONSULT", Name = "Consultant Request" };
            var ict = new RequestType { Code = "ICT", Name = "ICT Support" };

            db.RequestTypes.AddRange(cr, ict);
            await db.SaveChangesAsync();

            // Minimal workflow templates with 2–3 steps
            var crTemplate = new WorkflowTemplate { RequestTypeId = cr.Id, Code = "CONSULT_V1", Name = "Consultant Workflow" };
            var ictTemplate = new WorkflowTemplate { RequestTypeId = ict.Id, Code = "ICT_V1", Name = "ICT Support Workflow" };

            db.WorkflowTemplates.AddRange(crTemplate, ictTemplate);
            await db.SaveChangesAsync();

            db.WorkflowStepTemplates.AddRange(
                new WorkflowStepTemplate { WorkflowTemplateId = crTemplate.Id, StepOrder = 1, Code = "CR_SUBMIT", Name = "Submit", AssignmentMode = Domain.Enums.AssignmentMode.AutoAssign, AllowCreatorOrPreparer = true },
                new WorkflowStepTemplate { WorkflowTemplateId = crTemplate.Id, StepOrder = 2, Code = "CR_REVIEW", Name = "Review", AssignmentMode = Domain.Enums.AssignmentMode.RoleBased, AllowedRolesCsv = "Reviewer" },
                new WorkflowStepTemplate { WorkflowTemplateId = crTemplate.Id, StepOrder = 3, Code = "CR_APPROVE", Name = "Approve", AssignmentMode = Domain.Enums.AssignmentMode.RoleBased, AllowedRolesCsv = "ProcessOwner" },

                new WorkflowStepTemplate { WorkflowTemplateId = ictTemplate.Id, StepOrder = 1, Code = "ICT_SUBMIT", Name = "Submit", AssignmentMode = Domain.Enums.AssignmentMode.AutoAssign, AllowCreatorOrPreparer = true },
                new WorkflowStepTemplate { WorkflowTemplateId = ictTemplate.Id, StepOrder = 2, Code = "ICT_ASSIGN", Name = "Assign", AssignmentMode = Domain.Enums.AssignmentMode.SelectedByPreviousStep },
                new WorkflowStepTemplate { WorkflowTemplateId = ictTemplate.Id, StepOrder = 3, Code = "ICT_RESOLVE", Name = "Resolve", AssignmentMode = Domain.Enums.AssignmentMode.RoleBased, AllowedRolesCsv = "ICTAgent" }
            );

            await db.SaveChangesAsync();
        }
    }
}
