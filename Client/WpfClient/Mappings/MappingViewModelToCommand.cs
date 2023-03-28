using AutoMapper;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class MappingViewModelToCommand : Profile
    {
        public MappingViewModelToCommand()
        {
            CreateMap<RtuUpdateViewModel, UpdateRtu>();
            CreateMap<ZoneViewModel, AddZone>();
            CreateMap<ZoneViewModel, UpdateZone>();
        }
    }
}