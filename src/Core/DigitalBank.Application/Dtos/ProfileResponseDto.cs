namespace DigitalBank.Application.Dtos
{
    public class ProfileResponseDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }

        public string? AvatarUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
