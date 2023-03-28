using Fibertest.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Fibertest.GrpcClientLib
{
    public interface IGrpcHost
    {
        public  ClientGrpcData ClientGrpcData { get; set; }
        public void Start();
    }

    public class ClientGrpcServiceHost : IGrpcHost
    {
        private WebApplication? _app;
        public  ClientGrpcData ClientGrpcData { get; set; } = new();

        public void Start()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP((int)TcpPorts.ClientListenTo, o => o.Protocols = HttpProtocols.Http2);
            });

            builder.Services.AddGrpc();
            builder.Services.AddSingleton(ClientGrpcData); // means: Register instance

            _app = builder.Build();
            _app.MapGrpcService<ClientGrpcService>();
            _app.Run();
        }

    }
}
