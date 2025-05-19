using AutoMapper;
using DTO;
using Entities.models;
using MongoDB.Bson;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // DTO -> Entity (ממיר string ל-ObjectId)
        CreateMap<PasswordsDTO, Passwords>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new ObjectId(src.Id)));

        CreateMap<UsersDTO, Users>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new ObjectId(src.Id)));

        CreateMap<WebSitesDTO, WebSites>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new ObjectId(src.Id)));
        Console.WriteLine("mappingProfile");
        // Entity -> DTO (ממיר ObjectId ל-string)
        CreateMap<Passwords, PasswordsDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<Users, UsersDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<WebSites, WebSitesDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
    }
}
