using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;

namespace DigitalBank.Domain.Entities;

public class AuditLog : BaseEntity
{
    // əməliyyatı edən user (anonim ola bilər, məsələn login fail)
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public AuditActionType ActionType { get; set; }

    // Pul əməliyyatı ilə bağlamaq üçün (opsional)
    public int? RelatedTransactionId { get; set; }
    public BankTransaction? RelatedTransaction { get; set; }

    public bool IsSuccess { get; set; }

    public string? Description { get; set; } // qısa izah
    public string? DetailsJson { get; set; } // istəsən əlavə payload saxla

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
}
