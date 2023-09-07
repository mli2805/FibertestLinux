using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Fibertest.DataCenter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost
            .ConfigureKestrel(options =>
            {
                // grpc
                options.ListenAnyIP((int)TcpPorts.ServerListenToCommonClient, o => o.Protocols = HttpProtocols.Http2);
                options.ListenAnyIP((int)TcpPorts.ServerListenToRtu, o => o.Protocols = HttpProtocols.Http2);
                // http
                options.ListenAnyIP((int)TcpPorts.WebApiListenTo, o => o.Protocols = HttpProtocols.Http1);
            });

        // builder.Services.AddCors(options => options.AddPolicy("Cors", builder =>
        // {
        //     builder
        //         .AllowAnyMethod()
        //         .AllowAnyHeader()
        //         .AllowCredentials()
        //         .SetIsOriginAllowed(hostName => true);
        // }));

        builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
        }));

        // Add services to the container.
        builder.Services.AddGrpc(o =>
        {
            o.Interceptors.Add<DcLoggerInterceptor>();
        });

        builder.Services.AddControllers();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
                // for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/webApiSignalRHub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddSignalR();

        builder.Services
           .AddDependencies();

        // var logLevel = builder.Configuration.GetSection("General")["LogLevel"];
        Logger logger = LoggerConfigurationFactory
            .Configure(LogEventLevel.Debug) // here is my configuration of log files
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        var app = builder.Build();
        app.UseRouting();
        app.UseCors();

        app.MapGrpcService<C2DService>()
            .RequireCors("AllowAll");
        app.MapGrpcService<C2RService>()
                .RequireCors("AllowAll");
        app.MapGrpcService<R2DService>()
                .RequireCors("AllowAll");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // check it: http://localhost:11080/misc/checkapi
        });

        app.Run();
    }
}