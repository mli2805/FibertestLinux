using Fibertest.Dto;
using Fibertest.Graph;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly ILogger<Web2DCommandsProcessor> _logger;
        private readonly Model _writeModel;
        private readonly ClientCollection _clientCollection;
        private readonly C2DCommandsProcessor _c2DCommandsProcessor;
        private readonly C2RCommandsProcessor _c2RCommandsProcessor;

        public Web2DCommandsProcessor(ILogger<Web2DCommandsProcessor> logger, 
            Model writeModel, ClientCollection clientCollection, 
            C2DCommandsProcessor c2DCommandsProcessor, C2RCommandsProcessor c2RCommandsProcessor)
        {
            _logger = logger;
            _writeModel = writeModel;
            _clientCollection = clientCollection;
            _c2DCommandsProcessor = c2DCommandsProcessor;
            _c2RCommandsProcessor = c2RCommandsProcessor;
        }

        public async Task<RequestAnswer> SetRtuOccupationState(OccupyRtuDto dto)
        {
            SetRtuOccupationDto dto2 = new SetRtuOccupationDto() 
                { RtuId = dto.RtuId, State = dto.State, ClientConnectionId = dto.ClientConnectionId };
            return await _c2DCommandsProcessor.ExecuteRequest(dto2);
        }
    }
}
