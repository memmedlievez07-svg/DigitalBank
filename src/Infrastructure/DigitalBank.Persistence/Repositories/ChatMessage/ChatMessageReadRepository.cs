using DigitalBank.Application.Repositories.ChatMessage;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.ChatMessage
{
    public class ChatMessageReadRepository : ReadRepository<DigitalBank.Domain.Entities.ChatMessage>,IChatMessageReadRepository
    {
        public ChatMessageReadRepository(DigitalBankDbContext context):base (context) { }
    
    }
}
