using EORequests.Application.DTOs.Sla;
using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public sealed class SlaAdminService : ISlaAdminService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<SlaAdminService> _log;

        public SlaAdminService(EoDbContext db, ILogger<SlaAdminService> log)
        {
            _db = db; _log = log;
        }

        public async Task<SlaRuleDto?> GetForStepAsync(Guid workflowStepTemplateId, CancellationToken ct = default)
        {
            var rule = await _db.SlaRules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkflowStepTemplateId == workflowStepTemplateId, ct);

            return rule is null ? null : new SlaRuleDto
            {
                Id = rule.Id,
                WorkflowStepTemplateId = rule.WorkflowStepTemplateId,
                DueDays = rule.DueDays,
                ReminderOffsetsCsv = rule.ReminderOffsetsCsv,
                EscalationOffsetDays = rule.EscalationOffsetDays,
                IsActive = rule.IsActive,
                RowVersion = rule.RowVersion
            };
        }

        public async Task<SlaRuleDto> UpsertForStepAsync(SlaRuleUpsertDto dto, CancellationToken ct = default)
        {
            Validate(dto);

            var rule = await _db.SlaRules
                .FirstOrDefaultAsync(x => x.WorkflowStepTemplateId == dto.WorkflowStepTemplateId, ct);

            if (rule is null)
            {
                rule = new SlaRule
                {
                    WorkflowStepTemplateId = dto.WorkflowStepTemplateId,
                    DueDays = dto.DueDays,
                    ReminderOffsetsCsv = NormalizeCsv(dto.ReminderOffsetsCsv),
                    EscalationOffsetDays = dto.EscalationOffsetDays,
                    IsActive = dto.IsActive
                };
                _db.SlaRules.Add(rule);
            }
            else
            {
                // optimistic concurrency
                if (dto.RowVersion is not null)
                    _db.Entry(rule).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

                rule.DueDays = dto.DueDays;
                rule.ReminderOffsetsCsv = NormalizeCsv(dto.ReminderOffsetsCsv);
                rule.EscalationOffsetDays = dto.EscalationOffsetDays;
                rule.IsActive = dto.IsActive;
            }

            await _db.SaveChangesAsync(ct);

            return new SlaRuleDto
            {
                Id = rule.Id,
                WorkflowStepTemplateId = rule.WorkflowStepTemplateId,
                DueDays = rule.DueDays,
                ReminderOffsetsCsv = rule.ReminderOffsetsCsv,
                EscalationOffsetDays = rule.EscalationOffsetDays,
                IsActive = rule.IsActive,
                RowVersion = rule.RowVersion
            };
        }

        private static void Validate(SlaRuleUpsertDto dto)
        {
            if (dto.DueDays <= 0) throw new ArgumentOutOfRangeException(nameof(dto.DueDays));

            var reminders = ParseOffsets(dto.ReminderOffsetsCsv);
            if (reminders.Any(r => r <= 0 || r >= dto.DueDays))
                throw new InvalidOperationException("Each reminder offset must be > 0 and strictly less than DueDays.");

            if (dto.EscalationOffsetDays.HasValue && dto.EscalationOffsetDays.Value <= 0)
                throw new InvalidOperationException("EscalationOffsetDays, if provided, must be > 0.");
        }

        private static string NormalizeCsv(string csv)
        {
            var ints = ParseOffsets(csv);
            return string.Join(",", ints.OrderByDescending(x => x)); // e.g., "3,1" (T-3 then T-1)
        }

        private static IEnumerable<int> ParseOffsets(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return Enumerable.Empty<int>();

            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Select(s => int.Parse(s, CultureInfo.InvariantCulture));
        }
    }
}
