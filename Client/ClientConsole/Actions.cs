using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.StringResources;
using Newtonsoft.Json;

namespace ClientConsole;

public static class Actions
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
    private static readonly string _username = "Vasya Pugovkin";
    private static readonly string _password = "123";
    private static readonly string _clientId = "client-connection-id";
    private static readonly string _clientIP = "<<wpf address IP>>";

    public static async Task Do(int action, c2r.c2rClient grpcClient, c2d.c2dClient c2dClient)
    {
        switch (action)
        {
            case 1: await InitDllsAndConnectOtdr(grpcClient); break;
            case 2: await DisconnectOtdr(grpcClient); break;
            case 3: var unused = await RegisterClient(c2dClient); break;
        }
    }

    private static async Task<ClientRegisteredDto?> RegisterClient(c2d.c2dClient grpcClient)
    {
        var dto = new RegisterClientDto(_username, _password ) { ClientIp = _clientIP, ConnectionId = _clientId};
        var command = new c2dCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };
        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<ClientRegisteredDto>(response.Json);
            Console.WriteLine(response.Json);
            if (result != null)
                Console.WriteLine(result.ReturnCode);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task InitDllsAndConnectOtdr(c2r.c2rClient grpcClient)
    {
        var dto = new InitializeRtuDto(Guid.NewGuid(), RtuMaker.IIT) { ClientConnectionId = _clientId};
        var command = new c2rCommand()
        { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };
        Console.WriteLine(Resources.SID_long_operation_please_wait);

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
        var dto = new FreeOtdrDto(Guid.NewGuid(), RtuMaker.IIT) { ClientConnectionId = _clientId };
        var command = new c2rCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
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