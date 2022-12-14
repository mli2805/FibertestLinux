using System.Net;
using Fibertest.Dto;
using Fibertest.Utils;
using Serilog;

namespace Fibertest.Rtu;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost
            .ConfigureKestrel(o =>
            {
                o.Listen(IPAddress.Any, (int)TcpPorts.RtuListenTo);
            })
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.Configure();
            });

        // Add services to the container.
        builder.Services.AddGrpc(o =>
        {
            o.Interceptors.Add<RtuLoggerInterceptor>();
        });

        builder.Services
            .AddConfig(builder.Configuration)
            .AddDependencyGroup();

        var logLevel = builder.Configuration.GetSection("General")["LogLevel"];
        var logger = LoggerConfigurationFactory.Configure(logLevel).CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<RtuGrpcService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}