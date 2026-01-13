using DigitalBank.Application.Repositories.ChatMessage;
using DigitalBank.Persistence.Dal;
namespace DigitalBank.Persistence.Repositories.ChatMessage
{
    public class ChatMessageWriteRepository:WriteRepository<DigitalBank.Domain.Entities.ChatMessage>,IChatMessageWriteRepository
    {
        public ChatMessageWriteRepository(DigitalBankDbContext context) : base(context) { }

    }
}
