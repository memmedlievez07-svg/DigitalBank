using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;

namespace DigitalBank.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Opsional: bildiriş hansı əməliyyatla bağlıdır (məs: transfer gəldi)
    public int? RelatedTransactionId { get; set; }
    public BankTransaction? RelatedTransaction { get; set; }
}
