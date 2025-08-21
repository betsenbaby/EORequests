using EORequests.Domain.Audit;


namespace EORequests.Domain.Entities
{
    public class FormResponse : AuditableEntity
    {
        public Guid WorkflowStateId { get; set; }
        public string JsonData { get; set; } = "{}";

        // optional denormalizations
        public string? Summary { get; set; } // short text used in list views

        public WorkflowState WorkflowState { get; set; } = default!; //which step this response belongs to
        public string? SchemaVersionCaptured { get; set; } // which version of the schema was active when this response was captured

    }
}
