using AutoMapper;

namespace Fibertest.Graph;

public class MappingModelToCmdProfile : Profile
{
    public MappingModelToCmdProfile()
    {
        CreateMap<User, UpdateUser>();
        CreateMap<User, AssignUsersMachineKey>();
    }
}