using AutoMapper;
using DigitalBank.Application.Dtos;
using DigitalBank.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Mapping
{
    public class TransactionMap : Profile
    {
        public TransactionMap()
        {
            CreateMap<BankTransaction, TransactionListItemDto>()
           .ForMember(d => d.Type, o => o.MapFrom(s => (int)s.Type))
           .ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status))
           .ForMember(d => d.CreatedDateUtc, o => o.MapFrom(s => s.CreatedDate));

            CreateMap<BankTransaction, TransferResponseDto>()
                .ForMember(d => d.CreatedDateUtc, o => o.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.SenderWalletId, o => o.MapFrom(s => s.SenderWalletId ?? 0))
                .ForMember(d => d.ReceiverWalletId, o => o.MapFrom(s => s.ReceiverWalletId ?? 0));
        }
    }
}
