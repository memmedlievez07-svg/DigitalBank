namespace DigitalBank.Application.Dtos
{
    public class AdminUserListItemDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEndUtc { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
