using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Jobs
{
    public class PreviewCleanupJob
    {
        private readonly EoDbContext _db;
        private readonly ILogger<PreviewCleanupJob> _log;

        // keep at least a week by default
        private readonly int _retentionDays;

        public PreviewCleanupJob(EoDbContext db, ILogger<PreviewCleanupJob> log, int retentionDays = 7)
        {
            _db = db; _log = log; _retentionDays = Math.Max(1, retentionDays);
        }

        /// <summary>
        /// Deletes preview Requests older than retention, including their instances/states (via FK cascade).
        /// </summary>
        public async Task RunAsync(CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);

            // collect targets
            var oldPreviewIds = await _db.Requests
                .Where(r => r.IsPreview && (r.PreviewCreatedOn ?? r.CreatedOn) < cutoff)
                .Select(r => r.Id)
                .ToListAsync(ct);

            if (oldPreviewIds.Count == 0)
            {
                _log.LogInformation("Preview cleanup: nothing to delete (retention {Days}d).", _retentionDays);
                return;
            }

            // EF bulk delete (EF Core 7/8 supports ExecuteDeleteAsync on SQL providers)
            var deleted = await _db.Requests
                .Where(r => oldPreviewIds.Contains(r.Id))
                .ExecuteDeleteAsync(ct);

            _log.LogInformation("Preview cleanup: deleted {Count} preview request(s) older than {Cutoff:u}.",
                deleted, cutoff);
        }
    }
}
