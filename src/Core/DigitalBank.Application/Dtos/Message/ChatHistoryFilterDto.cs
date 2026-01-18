
namespace DigitalBank.Application.Dtos.Message
{
    public class ChatHistoryFilterDto
    {

        public string PeerUserId { get; set; } = null!;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
