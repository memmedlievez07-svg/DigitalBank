using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Options
{
    public class SmtpOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = "DigitalBank";
    }
}
