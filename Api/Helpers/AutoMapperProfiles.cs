using System.Linq;
using Api.DTOs;
using Api.Entities;
using AutoMapper;

namespace Api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(
                    dest => dest.PhotoUrl, 
                    options => options.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
        }
    }
}