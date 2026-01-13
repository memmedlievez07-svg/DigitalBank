namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class LatestAuditDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int ActionType { get; set; }
        public bool IsSuccess { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? CorrelationId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
