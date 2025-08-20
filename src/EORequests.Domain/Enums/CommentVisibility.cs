

namespace EORequests.Domain.Enums
{
    public enum CommentVisibility
    {
        Internal = 1,          // staff-only
        RequesterVisible = 2   // visible to submitter + authorized roles
    }
}
