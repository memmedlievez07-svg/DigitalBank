using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null;

        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
    }
}
