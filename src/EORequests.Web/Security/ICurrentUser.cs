namespace EORequests.Web.Security
{
    public interface ICurrentUser
    {
        Guid? TryGetId();
        Guid GetIdOrThrow();           // throws if missing
        string? Email();
        IReadOnlyCollection<string> Roles();
    }
}
