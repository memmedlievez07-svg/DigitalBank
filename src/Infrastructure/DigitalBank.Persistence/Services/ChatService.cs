using DigitalBank.Application.Dtos.Message;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;
        private readonly IChatPushService _chatPush;
        private readonly INotificationPushService _notifPush;

        public ChatService(
            IUnitOfWork uow,
            ICurrentUserContext current,
            IChatPushService chatPush,
            INotificationPushService notifPush)
        {
            _uow = uow;
            _current = current;
            _chatPush = chatPush;
            _notifPush = notifPush;
        }

        public async Task<ServiceResultVoid> SendAsync(SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            if (string.IsNullOrWhiteSpace(dto.ReceiverUserId))
                return ServiceResultVoid.Fail("ReceiverUserId is required", 400);

            if (string.IsNullOrWhiteSpace(dto.Message))
                return ServiceResultVoid.Fail("Message is required", 400);

            var senderId = _current.UserId!;
            var receiverId = dto.ReceiverUserId;

            var msg = new ChatMessage
            {
                SenderUserId = senderId,
                ReceiverUserId = receiverId,
                Message = dto.Message,
                IsRead = false,
                ReadAt = null
            };

            await _uow.ChatMessageWriteRepository.AddAsync(msg);

            // 🔔 create notification for receiver
            var notif = new Notification
            {
                UserId = receiverId,
                Type = NotificationType.ChatMessage,
                Title = "New message",
                Body = dto.Message.Length > 80 ? dto.Message.Substring(0, 80) + "..." : dto.Message,
                IsRead = false,
                ReadAt = null
            };

            await _uow.NotificationWriteRepository.AddAsync(notif);

            // ✅ 1 dəfə commit
            await _uow.CommitAsync();

            // ✅ real-time chat push (if online)
            await _chatPush.PushMessageAsync(receiverId, new
            {
                id = msg.Id,
                senderUserId = senderId,
                receiverUserId = receiverId,
                message = msg.Message,
                createdDate = msg.CreatedDate
            });

            // ✅ real-time notification push + unread count trigger
            await _notifPush.PushToUserAsync(receiverId, new
            {
                title = notif.Title,
                body = notif.Body,
                type = (int)notif.Type
            });

            await _notifPush.PushUnreadCountChangedAsync(receiverId);

            return ServiceResultVoid.Ok("Message sent");
        }

        public async Task<ServiceResult<PagedResult<ChatMessageDto>>> GetHistoryAsync(ChatHistoryFilterDto filter)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<PagedResult<ChatMessageDto>>.Fail("Unauthorized", 401);

            var me = _current.UserId!;
            var peer = filter.PeerUserId;

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            var q = _uow.ChatMessageReadRepository.Table
                .AsNoTracking()
                .Where(m =>
                    (m.SenderUserId == me && m.ReceiverUserId == peer) ||
                    (m.SenderUserId == peer && m.ReceiverUserId == me)
                );

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    ReceiverUserId = m.ReceiverUserId,
                    Message = m.Message,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            return ServiceResult<PagedResult<ChatMessageDto>>.Ok(new PagedResult<ChatMessageDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResultVoid> MarkReadAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            var me = _current.UserId!;

            // tracking lazımdır (update edirik)
            var msg = await _uow.ChatMessageReadRepository.Table
                .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverUserId == me);

            if (msg == null)
                return ServiceResultVoid.Fail("Message not found", 404);

            if (!msg.IsRead)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;

                _uow.ChatMessageWriteRepository.Update(msg);
                await _uow.CommitAsync();
            }

            return ServiceResultVoid.Ok("Marked read");
        }
    }
}
