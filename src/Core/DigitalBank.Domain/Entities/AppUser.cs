using DigitalBank.Domain.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DigitalBank.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }

        // Fayl DB-də saxlanmır. Sadəcə url/path saxlaya bilərik (opsional).
        public string? AvatarUrl { get; set; }

        // Navigation
        public Wallet? Wallet { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
