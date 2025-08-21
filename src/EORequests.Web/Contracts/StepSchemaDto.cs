namespace EORequests.Web.Contracts
{
    public sealed class StepSchemaDto
    {
        public Guid StepTemplateId { get; set; }
        public string? JsonSchema { get; set; }
    }
}
