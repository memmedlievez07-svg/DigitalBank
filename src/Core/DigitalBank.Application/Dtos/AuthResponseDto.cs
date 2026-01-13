using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAtUtc { get; set; }

        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiresAtUtc { get; set; }

        public UserBriefDto User { get; set; } = null!;
    }
}
