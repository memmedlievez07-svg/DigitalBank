namespace DigitalBank.Application.Dtos
{
    public class ProfileUpdateRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
    }
}
