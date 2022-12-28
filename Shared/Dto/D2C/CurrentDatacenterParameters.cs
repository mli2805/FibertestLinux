namespace Fibertest.Dto
{
    public class CurrentDatacenterParameters
    {
        public ServerGeneralDto ServerGeneral = new ServerGeneralDto();
        public EventSourcingDto EventSourcing = new EventSourcingDto();
        public WebApiDto? WebApi;

        public SmtpSettingsDto Smtp = new SmtpSettingsDto();
        public SnmpSettingsDto? Snmp;
    }


}