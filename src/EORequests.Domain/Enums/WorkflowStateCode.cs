

namespace EORequests.Domain.Enums
{
    public enum WorkflowStateCode
    {
        NotStarted = 0,
        PendingAction = 1,
        WaitingForInfo = 2,
        Completed = 3,
        Skipped = 4,
        Cancelled = 5
    }
}
