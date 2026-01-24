using DigitalBank.Application.Dtos;
using DigitalBank.Application.Dtos.Message;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;
        private readonly IChatPushService _chatPush;
        private readonly INotificationPushService _notifPush;
        private readonly UserManager<AppUser> _userManager;

        public ChatService(
            IUnitOfWork uow,
            ICurrentUserContext current,
            IChatPushService chatPush,
            INotificationPushService notifPush,
            UserManager<AppUser> userManager)
        {
            _uow = uow;
            _current = current;
            _chatPush = chatPush;
            _notifPush = notifPush;
            _userManager = userManager;
        }

        public async Task<ServiceResultVoid> SendAsync(SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            var senderId = _current.UserId!;

            // 1. Mesaj obyektini yaradırıq
            var msg = new ChatMessage
            {
                SenderUserId = senderId,
                ReceiverUserId = dto.ReceiverUserId,
                Message = dto.Message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            // 2. Bazaya yazırıq
            await _uow.ChatMessageWriteRepository.AddAsync(msg);
            await _uow.CommitAsync();

            // 3. SignalR üçün DTO hazırlayırıq (Frontend bu formatı gözləyir)
            var msgDto = new ChatMessageDto
            {
                Id = msg.Id,
                SenderUserId = msg.SenderUserId,
                ReceiverUserId = msg.ReceiverUserId,
                Message = msg.Message,
                CreatedDate = msg.CreatedDate,
                IsRead = false
            };

            // 4. REAL-TIME PUSH (Həm Chat, həm Notification)
            // Alıcıya mesajı göndər
            await _chatPush.PushMessageAsync(dto.ReceiverUserId, msgDto);

            // Alıcıya bildiriş (notification) göndər ki, yuxarıda "pop-up" çıxsın
            //await _notifPush.PushToUserAsync(dto.ReceiverUserId, new
            //{
            //    title = "Yeni mesaj",
            //    body = dto.Message.Length > 30 ? dto.Message.Substring(0, 30) + "..." : dto.Message,
            //    type = (int)NotificationType.ChatMessage
            //});

            return ServiceResultVoid.Ok("Mesaj göndərildi");
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
        public async Task<ServiceResult<List<UserBriefDto>>> GetMyConversationsAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<List<UserBriefDto>>.Fail("Unauthorized", 401);

            var me = _current.UserId;

            // 1. Mənim iştirak etdiyim mesajlardan unikal istifadəçi ID-lərini tapırıq
            var userIds = await _uow.ChatMessageReadRepository.Table
                .Where(m => m.SenderUserId == me || m.ReceiverUserId == me)
                .Select(m => m.SenderUserId == me ? m.ReceiverUserId : m.SenderUserId)
                .Distinct()
                .ToListAsync();

            var users = new List<UserBriefDto>();

            foreach (var id in userIds)
            {
                var u = await _userManager.FindByIdAsync(id);
                if (u != null)
                {
                    // Bu istifadəçidən mənə gələn və oxunmamış (IsRead == false) mesaj varammı?
                    // Qeyd: ChatMessage modelində 'IsRead' sütununun olduğunu fərz edirəm
                    bool unread = await _uow.ChatMessageReadRepository.Table
                        .AnyAsync(m => m.SenderUserId == id && m.ReceiverUserId == me && !m.IsRead);

                    users.Add(new UserBriefDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        UserName = u.UserName,
                        Email = u.Email,
                        HasUnread = unread 
                    });
                }
            }

            return ServiceResult<List<UserBriefDto>>.Ok(users);
        }

   
        public async Task<ServiceResultVoid> MarkAllReadAsync(string peerUserId)
        {
            var me = _current.UserId;
            var messages = await _uow.ChatMessageReadRepository.Table
                .Where(x => x.ReceiverUserId == me && x.SenderUserId == peerUserId && !x.IsRead)
                .ToListAsync();

            foreach (var m in messages)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.UtcNow;
                _uow.ChatMessageWriteRepository.Update(m);
            }
            await _uow.CommitAsync();
            return ServiceResultVoid.Ok();
        }
    }
}
