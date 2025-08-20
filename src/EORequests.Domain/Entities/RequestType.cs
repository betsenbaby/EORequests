using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class RequestType : AuditableEntity
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public ICollection<Request> Requests { get; set; } = new List<Request>();
        public ICollection<WorkflowTemplate> WorkflowTemplates { get; set; } = new List<WorkflowTemplate>();
    }
}
