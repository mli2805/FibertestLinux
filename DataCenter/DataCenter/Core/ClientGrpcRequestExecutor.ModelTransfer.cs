using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using NEventStore;

namespace Fibertest.DataCenter
{
    public partial class ClientGrpcRequestExecutor
    {
        private byte[]? _serializedModel;
        private const int PortionSize = 2 * 1024 * 1024;
        static readonly object LockObj = new object();
        public async Task<SerializedModelDto> GetModelDownloadParams()
        {
            await Task.Delay(1);
            _logger.LogInfo(Logs.DataCenter, "Model asked by client");
            lock (LockObj)
            {
                _serializedModel = _writeModel.Serialize(_logger).Result;
            }

            if (_serializedModel == null)
            {
                _logger.LogError(Logs.DataCenter, "Failed to serialize Model");
                return new SerializedModelDto() { ReturnCode = ReturnCode.Error };

            }
            _logger.LogInfo(Logs.DataCenter, "Model serialized successfully");

            return new SerializedModelDto()
            {
                ReturnCode = ReturnCode.Ok,
                PortionsCount = _serializedModel.Length / PortionSize + 1,
                Size = _serializedModel.Length,
                LastIncludedEvent = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.StreamIdOriginal).StreamRevision,
            };
        }

        public async Task<SerializedModelPortionDto> GetModelPortion(int portionOrdinal)
        {
            await Task.Delay(1);
            var currentPortionSize = PortionSize * (portionOrdinal + 1) < _serializedModel!.Length
                ? PortionSize
                : _serializedModel.Length - PortionSize * (portionOrdinal);
            var portion = new byte[currentPortionSize];
            Array.Copy(_serializedModel, PortionSize * portionOrdinal,
                portion, 0, currentPortionSize);

            return new SerializedModelPortionDto()
            {
                ReturnCode = ReturnCode.Ok,
                Bytes = portion
            };
        }
    }
}
