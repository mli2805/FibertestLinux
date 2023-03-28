using AutoMapper;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class UserMappings : Profile
    {
        public UserMappings()
        {
            CreateMap<User, UserVm>();
            CreateMap<UserVm, User>();
        }
    }
}