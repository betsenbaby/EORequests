using EORequests.Domain.Entities;
using EORequests.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EORequests.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(EoDbContext db, CancellationToken ct = default)
    {
        // --- Roles: only those needed for Consultant workflow ---
        var roleNames = new[] { "Reviewer", "ProcessOwner", "Admin" };
        var existingRoles = await db.ApplicationRoles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Name)
            .ToListAsync(ct);

        foreach (var rn in roleNames.Except(existingRoles))
            db.ApplicationRoles.Add(new Domain.Security.ApplicationRole { Name = rn });

        await db.SaveChangesAsync(ct);

        // --- Request Type: Consultant ---
        var consult = await db.RequestTypes.FirstOrDefaultAsync(x => x.Code == "CONSULT", ct);
        if (consult == null)
        {
            consult = new RequestType { Code = "CONSULT", Name = "Consultant Request" };
            db.RequestTypes.Add(consult);
            await db.SaveChangesAsync(ct);
        }

        // --- Workflow Template: Consultant ---
        var consultWf = await db.WorkflowTemplates
            .FirstOrDefaultAsync(x => x.RequestTypeId == consult.Id && x.Code == "CONSULT_V1", ct);
        if (consultWf == null)
        {
            consultWf = new WorkflowTemplate
            {
                RequestTypeId = consult.Id,
                Code = "CONSULT_V1",
                Name = "Consultant Workflow V1"
            };
            db.WorkflowTemplates.Add(consultWf);
            await db.SaveChangesAsync(ct);
        }

        // --- Steps: Consultant workflow ---
        async Task EnsureStepAsync(Guid wfId, int order, string code, string name,
            AssignmentMode mode, string? allowedRoles = null, bool allowCreator = false)
        {
            var exists = await db.WorkflowStepTemplates
                .AnyAsync(s => s.WorkflowTemplateId == wfId && s.StepOrder == order, ct);
            if (!exists)
            {
                db.WorkflowStepTemplates.Add(new WorkflowStepTemplate
                {
                    WorkflowTemplateId = wfId,
                    StepOrder = order,
                    Code = code,
                    Name = name,
                    AssignmentMode = mode,
                    AllowedRolesCsv = allowedRoles,
                    AllowCreatorOrPreparer = allowCreator
                });
            }
        }

        await EnsureStepAsync(consultWf.Id, 1, "CR_SUBMIT", "Submit", AssignmentMode.AutoAssign, null, allowCreator: true);
        await EnsureStepAsync(consultWf.Id, 2, "CR_REVIEW", "Review", AssignmentMode.RoleBased, "Reviewer");
        await EnsureStepAsync(consultWf.Id, 3, "CR_APPROVE", "Approve", AssignmentMode.RoleBased, "ProcessOwner");

        await db.SaveChangesAsync(ct);

        // --- Example SLA rules ---
        var consultSteps = await db.WorkflowStepTemplates
            .Where(s => s.WorkflowTemplateId == consultWf.Id)
            .OrderBy(s => s.StepOrder).ToListAsync(ct);

        async Task EnsureSlaAsync(Guid stepTemplateId, int dueDays)
        {
            var exists = await db.SlaRules.AnyAsync(s => s.WorkflowStepTemplateId == stepTemplateId && s.DueDays == dueDays, ct);
            if (!exists) db.SlaRules.Add(new SlaRule { WorkflowStepTemplateId = stepTemplateId, DueDays = dueDays });
        }

        if (consultSteps.Count > 1) await EnsureSlaAsync(consultSteps[1].Id, 3); // Review due in 3 days
        if (consultSteps.Count > 2) await EnsureSlaAsync(consultSteps[2].Id, 2); // Approve due in 2 days

        await db.SaveChangesAsync(ct);
    }
}
