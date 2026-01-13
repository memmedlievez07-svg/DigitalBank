using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;

namespace DigitalBank.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public string SenderUserId { get; set; } = null!;
    public AppUser SenderUser { get; set; } = null!;

    public string ReceiverUserId { get; set; } = null!;
    public AppUser ReceiverUser { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
