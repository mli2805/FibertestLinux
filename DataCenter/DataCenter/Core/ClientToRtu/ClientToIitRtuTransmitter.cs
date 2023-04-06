using Fibertest.Dto;
using Fibertest.Rtu;
using Fibertest.Utils;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class ClientToIitRtuTransmitter
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    private readonly ILogger<ClientToIitRtuTransmitter> _logger;

    public ClientToIitRtuTransmitter(ILogger<ClientToIitRtuTransmitter> logger)
    {
        _logger = logger;
    }

    public async Task<string> TransferCommand(string rtuAddress, string commandContent)
    {
        var rtuUri = $"http://{rtuAddress}";
        using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
        //_logger.Debug(Logs.DataCenter, $"GrpcChannel for {rtuUri}");
        var grpcClientRtu = new d2r.d2rClient(grpcChannelRtu);
        // _logger.Debug(Logs.DataCenter, $"Command content {commandContent}");

        var rtuCommand = new d2rCommand() { Json = commandContent };

        try
        {
            d2rResponse response = await grpcClientRtu.SendCommandAsync(rtuCommand);
            _logger.Debug(Logs.DataCenter, $"Got gRPC response from RTU: {response.Json}");
            return response.Json;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.DataCenter, "TransferCommand: " + e.Message);
            if (e.InnerException != null)
                _logger.Error(Logs.DataCenter, "InnerException: " + e.InnerException.Message);

            object badResult;
            switch (JsonConvert.DeserializeObject(commandContent, JsonSerializerSettings))
            {
                case InitializeRtuDto _: badResult = 
                    new RtuInitializedDto(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
                case AttachOtauDto _: badResult = 
                    new OtauAttachedDto(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
                case DetachOtauDto _: badResult = 
                    new OtauDetachedDto(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
                case DoClientMeasurementDto _: badResult =
                    new ClientMeasurementStartedDto(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
                case AssignBaseRefsDto _: badResult =
                    new BaseRefAssignedDto(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
                // case ApplyMonitoringSettingsDto _:
                // case DoOutOfTurnPreciseMeasurementDto _:
                // case StopMonitoringDto _:
                // case FreeOtdrDto _:
                default: badResult = new RequestAnswer(ReturnCode.D2RGrpcOperationError) { ErrorMessage = e.Message }; break;
            }

            return JsonConvert.SerializeObject(badResult, JsonSerializerSettings);
        }
    }
}