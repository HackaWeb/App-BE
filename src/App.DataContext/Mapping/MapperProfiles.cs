using App.DataContext.Models;
using AutoMapper;

namespace App.DataContext.Mapping
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<User, Domain.Models.User>()
                .ReverseMap();

            CreateMap<Notification, Domain.Models.Notification>()
                .ReverseMap();

            CreateMap<Tag, Domain.Models.Tag>()
                .ReverseMap();

            CreateMap<UserTag, Domain.Models.UserTag>()
                .ReverseMap();

            CreateMap<Transaction, Domain.Models.Transaction>()
                .ReverseMap();

            CreateMap<Credential, Domain.Models.Credential>()
                .ReverseMap();
        }
    }
}
