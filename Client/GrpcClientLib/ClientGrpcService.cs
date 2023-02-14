using Fibertest.Dto;
using Grpc.Core;
using GrpsClientLib;
using Newtonsoft.Json;

namespace Fibertest.GrpcClientLib
{
    public class ClientGrpcService : toClient.toClientBase
    {
        private readonly ClientGrpcData _clientGrpcData;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public ClientGrpcService(ClientGrpcData clientGrpcData)
        {
            _clientGrpcData = clientGrpcData;
        }

        public override async Task<toClientResponse> SendCommand(toClientCommand command, ServerCallContext context)
        {
            try
            {
               _clientGrpcData.Raise(command.Json);

                return new toClientResponse()
                { Json = JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.Ok), JsonSerializerSettings) };
            }
            catch (Exception e)
            {
                await Task.Delay(0);
                return new toClientResponse
                {
                    Json = JsonConvert
                        .SerializeObject(
                            new RequestAnswer(ReturnCode.ToClientGrpcOperationError){ErrorMessage = e.Message},
                            JsonSerializerSettings)
                };
            }
        }
    }
}
