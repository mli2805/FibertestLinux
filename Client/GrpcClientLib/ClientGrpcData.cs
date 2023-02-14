namespace Fibertest.GrpcClientLib;

public class ClientGrpcData
{
    public delegate void GrpcMessageHandler(object sender, string json);
    public event GrpcMessageHandler? GrpcMessageReceived;

    public void Raise(string json)
    {
        GrpcMessageReceived?.Invoke(this, json);
    }
}