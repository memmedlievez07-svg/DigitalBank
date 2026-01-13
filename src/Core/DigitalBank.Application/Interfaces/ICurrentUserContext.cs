using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Interfaces
{
    public interface ICurrentUserContext
    {
        string? UserId { get; }
        string? IpAddress { get; }
        string? UserAgent { get; }
        string CorrelationId { get; }
    }
}
