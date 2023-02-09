using Fibertest.Dto;
using Fibertest.GrpcClientLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Fibertest.WpfClient
{
    public class ClientGrpcServiceStarter
    {
        private WebApplication? _app;

        public void StartClientGrpcListener()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP((int)TcpPorts.ClientListenTo, o => o.Protocols = HttpProtocols.Http2);

            });

            builder.Services.AddGrpc();

            _app = builder.Build();
            _app.MapGrpcService<ClientGrpcService>();
            _app.Run();
        }

    }
}
