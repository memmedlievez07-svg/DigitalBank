using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Domain.Entities
{
    public class RefreshToken:BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; } = null!;

        // Plain token saxlamırıq; hash saxlayırıq
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }

        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    }
}
