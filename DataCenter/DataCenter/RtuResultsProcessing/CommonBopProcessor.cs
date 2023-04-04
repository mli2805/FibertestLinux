using AutoMapper;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class CommonBopProcessor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly IFtSignalRClient _ftSignalRClient;

        public CommonBopProcessor(Model writeModel, 
            EventStoreService eventStoreService, IFtSignalRClient ftSignalRClient)
        {
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _ftSignalRClient = ftSignalRClient;
        }   

        public async Task PersistBopEvent(AddBopNetworkEvent cmd)
        {
            var result = await _eventStoreService.SendCommand(cmd, "system", "OnServer");
            if (string.IsNullOrEmpty(result))
            {
                var bopEvent = _writeModel.BopNetworkEvents.LastOrDefault();
                var signal = Mapper.Map<BopEventDto>(bopEvent);

                await _ftSignalRClient.NotifyAll("AddBopEvent", signal.ToCamelCaseJson());
                var unused = Task.Factory.StartNew(() => SendNotificationsAboutBop(cmd));
            }
        }

        private void SendNotificationsAboutBop(AddBopNetworkEvent e)
        {
            // TODO smtp, sms, snmp

        }
    }
}
