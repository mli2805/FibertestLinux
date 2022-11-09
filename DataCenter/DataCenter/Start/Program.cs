using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace Fibertest.DataCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(options =>
            {
                // grpc
                options.ListenAnyIP((int)TcpPorts.ServerListenToCommonClient, o => o.Protocols = HttpProtocols.Http2);
                // http
                options.ListenAnyIP((int)TcpPorts.WebApiListenTo, o => o.Protocols = HttpProtocols.Http1);
            });

            // Add services to the container.
            builder.Services.AddGrpc(o =>
            {
                o.Interceptors.Add<ServerLoggerInterceptor>();
            });
            builder.Services.AddControllers();

            // my Dependency Injection
            builder.Services
                .AddDbRepositories();

            var logger = LoggerConfigurationFactory
                .Configure() // here is my configuration of log files
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            var app = builder.Build();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<C2RService>();
                endpoints.MapGrpcService<R2DService>();
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}