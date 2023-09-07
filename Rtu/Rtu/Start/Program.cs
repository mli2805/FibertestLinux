using System.Net;
using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

namespace Fibertest.Rtu;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost
            .ConfigureKestrel(options =>
            {
                options.ListenAnyIP((int)TcpPorts.RtuListenToGrpc, o => o.Protocols = HttpProtocols.Http2);
                options.ListenAnyIP((int)TcpPorts.RtuListenToHttp, o => o.Protocols = HttpProtocols.Http1);
            });

        builder.Services.AddControllers();
        // Add services to the container.
        builder.Services.AddGrpc(o =>
        {
            o.Interceptors.Add<RtuLoggerInterceptor>();
        });

      
        builder.Services
            .AddDependencyGroup();

        var logLevel = LogEventLevel.Debug;
        var logger = LoggerConfigurationFactory.Configure(logLevel).CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        var app = builder.Build();
        app.UseRouting();
        app.UseCors();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<RtuGrpcService>().RequireCors("AllowAll");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // check it: http://localhost:11080/misc/checkapi
        });
        app.Run();
    }
}