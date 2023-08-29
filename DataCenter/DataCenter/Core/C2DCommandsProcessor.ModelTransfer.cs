using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using NEventStore;

namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        public byte[]? SerializedModel;
        private const int PortionSize = 2 * 1024 * 1024;
        static readonly object LockObj = new object();
        public async Task<SerializedModelDto> GetModelDownloadParams()
        {
            await Task.Delay(1);
            _logger.Info(Logs.DataCenter, "Model asked by client");
            lock (LockObj)
            {
                SerializedModel = _writeModel.Serialize(_logger).Result;
                //
                // var nnn = new Model();
                // var r = nnn.Deserialize(_logger, SerializedModel!).Result;
            }

            if (SerializedModel == null)
            {
                _logger.Error(Logs.DataCenter, "Failed to serialize Model");
                return new SerializedModelDto() { ReturnCode = ReturnCode.Error };
            }
            _logger.Info(Logs.DataCenter, "Model serialized successfully");

            return new SerializedModelDto()
            {
                ReturnCode = ReturnCode.Ok,
                PortionsCount = SerializedModel.Length / PortionSize + 1,
                Size = SerializedModel.Length,
                LastIncludedEvent = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.StreamIdOriginal).StreamRevision,
            };
        }
    }
}
