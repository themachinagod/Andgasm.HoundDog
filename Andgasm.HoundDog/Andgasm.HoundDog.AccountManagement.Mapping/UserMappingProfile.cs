﻿using Andgasm.HoundDog.AccountManagement.Database;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using AutoMapper;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Andgasm.HoundDog.AccountManagement.Mapping
{
    public class UserMappingProfile: Profile
    {
        public UserMappingProfile()
        {
            CreateMap<HoundDogUser, UserDTO>();
            CreateMap<UserDTO, HoundDogUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id == Guid.Empty ? Guid.NewGuid() : src.Id))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore());



        }
    }
}
