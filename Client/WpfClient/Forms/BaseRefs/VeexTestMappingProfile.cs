using AutoMapper;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class VeexTestMappingProfile : Profile
    {
        public VeexTestMappingProfile()
        {
            CreateMap<VeexTestCreatedDto, AddVeexTest>();
        }
    }
}