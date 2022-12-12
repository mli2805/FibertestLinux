using AutoMapper;
using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class VeexTestMappingProfile : Profile
    {
        public VeexTestMappingProfile()
        {
            CreateMap<VeexTestCreatedDto, AddVeexTest>();
        }
    }
}