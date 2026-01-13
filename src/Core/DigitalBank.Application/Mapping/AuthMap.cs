using AutoMapper;
using DigitalBank.Application.Dtos;
using DigitalBank.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Mapping
{
    public class AuthMap : Profile
    {
        public AuthMap()
        {
            CreateMap<AppUser, UserBriefDto>();
        }
    }
}
