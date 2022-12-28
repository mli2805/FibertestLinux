using AutoMapper;
using Fibertest.Dto;

namespace Fibertest.DataCenter;

public class MappingConfigProfile : Profile
{
    public MappingConfigProfile()
    {
        CreateMap<EventSourcingConfig, EventSourcingDto>();
        CreateMap<SmtpConfig, SmtpSettingsDto>();
        CreateMap<SnmpConfig, SnmpSettingsDto>();
        CreateMap<WebApiConfig, WebApiDto>();
    }
}