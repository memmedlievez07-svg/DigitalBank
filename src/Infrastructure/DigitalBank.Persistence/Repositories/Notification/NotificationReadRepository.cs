using DigitalBank.Application.Repositories.Notification;
using DigitalBank.Persistence.Dal;
namespace DigitalBank.Persistence.Repositories.Notification
{
    public class NotificationReadRepository : ReadRepository<DigitalBank.Domain.Entities.Notification>, INotificationReadRepository
    {
        public NotificationReadRepository(DigitalBankDbContext context) :base(context) { }
    }
}
