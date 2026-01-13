using AutoMapper;
using DigitalBank.Application.Dtos;
using DigitalBank.Domain.Entities;
namespace DigitalBank.Application.Mapping
{
    public class WalletMap:Profile
    {
        public WalletMap()
        {
            CreateMap<Wallet, WalletDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status));
        }
    }
}
