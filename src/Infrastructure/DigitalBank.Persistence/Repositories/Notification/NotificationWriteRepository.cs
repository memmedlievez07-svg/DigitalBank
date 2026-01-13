using DigitalBank.Application.Repositories.Notification;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.Notification
{
    public class NotificationWriteRepository : WriteRepository<DigitalBank.Domain.Entities.Notification>,INotificationWriteRepository
    {
        public NotificationWriteRepository(DigitalBankDbContext context) : base(context) { }
    }
}
