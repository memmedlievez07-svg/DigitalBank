namespace DigitalBank.Application.Interfaces
{
    public interface IWalletCodeGenerator
    {
        Task<string> GenerateUniqueCardNumberAsync();
    }
}
