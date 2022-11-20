using Fibertest.DataCenter;
using Fibertest.Dto;
using Newtonsoft.Json;
using StringResources;

namespace ClientConsole;

public static class Actions
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    public static async Task Do(int action, c2r.c2rClient grpcClient)
    {
        switch (action)
        {
            case 1: await InitDllsAndConnectOtdr(grpcClient); break;
            case 2: await DisconnectOtdr(grpcClient); break;
        }
    }

    private static async Task InitDllsAndConnectOtdr(c2r.c2rClient grpcClient)
    {
        var dto = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
        var command = new c2rCommand()
            { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };
        Console.WriteLine("длительная операция, пожалуйста подождите...");

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            Console.WriteLine(Resources.SID_DllInit_result_is_ + (result == null ? "null" : $"{result.IsInitialized}"));
            if (result != null)
                Console.WriteLine(Resources.SID_Serial_is__ + result.Serial);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task DisconnectOtdr(c2r.c2rClient grpcClient)
    {
        var dto = new FreeOtdrDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
        var command = new c2rCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<BaseRtuReply>(response.Json);
            Console.WriteLine(Resources.SID_FreeOtdr_result_is_ + (result == null ? "null" : $"{result.ReturnCode == ReturnCode.Ok}"));
            if (result != null)
                Console.WriteLine(Resources.SID_FreeOtdr_returned_ + result.ReturnCode);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}