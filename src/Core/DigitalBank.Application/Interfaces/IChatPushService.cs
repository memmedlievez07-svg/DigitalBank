namespace DigitalBank.Application.Interfaces
{
    public interface IChatPushService
    {
        Task PushMessageAsync(string receiverUserId, object payload);

    }
}
