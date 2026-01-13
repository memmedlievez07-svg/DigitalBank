using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class RefreshTokenRequestDto
    {
        // refresh token-i body-də saxlayırıq (sonraya cookie keçərik)
        public string RefreshToken { get; set; } = null!;
    }
}
