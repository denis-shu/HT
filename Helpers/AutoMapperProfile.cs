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
                        opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMainPhoto).Url);
                    })
                    .ForMember(dest => dest.Age, opt =>
                        {
                            opt.ResolveUsing(date => date.DateOfBirth.CalculateAge());
                        });


            CreateMap<User, UserForDetailDTO>()
            .ForMember(dest => dest.PhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMainPhoto).Url);
            })
            .ForMember(dest => dest.Age, opt =>
            {
                opt.ResolveUsing(date => date.DateOfBirth.CalculateAge());
            });
            CreateMap<Photo, PhotoForDetailDTO>();
            CreateMap<UserUpdateDTO, User>();
            CreateMap<PhotoDTO, Photo>();
            CreateMap<Photo, PhotoForReturn>();
            CreateMap<RegisterDto, User>();
            CreateMap<MessageForCreationDTO, Message>().ReverseMap();
            CreateMap<Message, MessageToReternDTO>()
                .ForMember(x => x.SenderPhotoUrl, opt =>
                    opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMainPhoto).Url))
                .ForMember(x => x.RecipientPhotoUrl, opt => 
                    opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMainPhoto).Url));
        }
    }
}