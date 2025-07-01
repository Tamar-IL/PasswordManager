//using AutoMapper;
//using DTO;
//using Entities.models;
//using MongoDB.Bson;
//using System.Text;

//public class MappingProfile : Profile
//{
//    public MappingProfile()
//    {
//        // DTO -> Entity (ממיר string ל-ObjectId)
//        CreateMap<PasswordsDTO, Passwords>()
//             .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
//                 string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId().ToString() : src.Id))
//             .ForMember(dest => dest.Password, opt => opt.Ignore()) 
//             .ReverseMap()
//             .ForMember(dest => dest.Password, opt => opt.MapFrom(src =>
//                 src.Password != null ? string.Join(",", src.Password) : string.Empty));

//        CreateMap<UsersDTO, Users>()
//           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new ObjectId(src.Id)));

//        CreateMap<Users, UsersDTO>()
//      .ForMember(dest => dest.Password, opt => opt.Ignore());

//        CreateMap<WebSitesDTO, WebSites>()
//           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new ObjectId(src.Id)));


//        Console.WriteLine("mappingProfile");



//        // Entity -> DTO (ממיר ObjectId ל-string)
//        CreateMap<Passwords, PasswordsDTO>()
//            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

//        CreateMap<Users, UsersDTO>()
//            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

//        CreateMap<WebSites, WebSitesDTO>()
//            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
//    }
//}
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
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // תתעלם - תוסיף ידנית
            .ReverseMap()
            .ForMember(dest => dest.Password, opt => opt.Ignore()); // ← שנה כאן!

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
