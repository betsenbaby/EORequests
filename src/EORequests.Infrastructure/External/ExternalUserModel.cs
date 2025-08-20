

namespace EORequests.Infrastructure.External
{
    public class ExternalUserModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? IndexNumber { get; set; }
        public bool Active { get; set; }
    }
}
