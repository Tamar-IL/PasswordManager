using AutoMapper;
using DTO;
using Entities;
using MongoDB.Bson;
namespace API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Users, UsersDTO>().ReverseMap();
            CreateMap<PasswordsDTO, Passwords>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                   string.IsNullOrEmpty(src.Id) || src.Id == "string" ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));

            CreateMap<Passwords, PasswordsDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}