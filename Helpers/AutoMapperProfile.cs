using AutoMapper;
using DatingApp.API.DTOS;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserForListDTO>()
                    .ForMember(dest => dest.PhotoUrl, opt =>
                    {
                        opt.MapFrom(src => src.Photos.FirstOrDefault(p => !p.IsMainPhoto).Url);
                    })
                    .ForMember(dest => dest.Age, opt =>
                        {
                            opt.ResolveUsing(date => date.DateOfBirth.CalculateAge());
                        });


            CreateMap<User, UserForDetailDTO>()
            .ForMember(dest => dest.PhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => !p.IsMainPhoto).Url);
            })
            .ForMember(dest => dest.Age, opt =>
            {
                opt.ResolveUsing(date => date.DateOfBirth.CalculateAge());
            });
            CreateMap<Photo, PhotoForDetailDTO>();
        }
    }
}