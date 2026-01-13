namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class LatestNotificationDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
