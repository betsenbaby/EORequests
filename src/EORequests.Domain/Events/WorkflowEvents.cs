namespace EORequests.Domain.Events;

public record StepActivated(Guid InstanceId, Guid StateId, Guid StepTemplateId, Guid? AssigneeUserId);
public record StepCompleted(Guid InstanceId, Guid StateId, Guid StepTemplateId, Guid CompletedByUserId);
public record AssignmentChanged(Guid InstanceId, Guid StateId, Guid? OldAssigneeUserId, Guid? NewAssigneeUserId);
