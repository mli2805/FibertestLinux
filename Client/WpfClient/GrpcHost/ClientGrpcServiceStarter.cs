using Fibertest.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fibertest.WpfClient
{
    public class ClientGrpcServiceStarter
    {
        private WebApplication? _app;

        private void StartClientGrpcListener()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls($"http://*:{TcpPorts.ClientListenTo}");

            builder.Services.AddGrpc();

            _app = builder.Build();
            _app.MapGrpcService<ClientGrpcService>();
            _app.Run();
        }

    }
}
