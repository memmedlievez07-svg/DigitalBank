using DigitalBank.Application.Dtos;
using DigitalBank.Application.Dtos.Message;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResultVoid> SendAsync(SendMessageDto dto);
        Task<ServiceResult<PagedResult<ChatMessageDto>>> GetHistoryAsync(ChatHistoryFilterDto filter);
        Task<ServiceResultVoid> MarkReadAsync(int id);
        Task<ServiceResult<List<UserBriefDto>>> GetMyConversationsAsync();

        Task<ServiceResultVoid> MarkAllReadAsync(string peerUserId);
       
    }
}
