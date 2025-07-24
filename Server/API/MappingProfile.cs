
using AutoMapper;
using DTO;
using Entities.models;
using MongoDB.Bson;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Passwords mapping
        CreateMap<PasswordsDTO, Passwords>()
     .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
         string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId().ToString() : src.Id))
     .ForMember(dest => dest.Password, opt => opt.MapFrom(src =>
         string.IsNullOrEmpty(src.Password) ? null : src.Password.Select(c => (int)c).ToArray()))
     .ReverseMap()
     .ForMember(dest => dest.Password, opt => opt.MapFrom(src =>
         src.Password == null ? null : new string(src.Password.Select(i => (char)i).ToArray())));

        // Users mapping
        CreateMap<UsersDTO, Users>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId().ToString() : src.Id))
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Password, opt => opt.Ignore());

        // WebSites mapping
        CreateMap<WebSitesDTO, WebSites>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId().ToString() : src.Id))
            .ReverseMap();

        Console.WriteLine("mappingProfile");
    }
}
