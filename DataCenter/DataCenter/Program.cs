using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Grpc.Core.Interceptors;
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

    public static class DependencyInjectionExtensions
    {
        // методов может быть несколько
        // а названия описывать объединенные зависимости
        public static IServiceCollection AddDbRepositories(this IServiceCollection services)
        {
            services.AddScoped<RtuRepo>(); // для каждого реквеста новый
            // services.AddScoped<class2>();
            // services.AddScoped<class3>();

            return services;
        }
    }

    public class ServerLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ServerLoggerInterceptor> _logger;

        public ServerLoggerInterceptor(ILogger<ServerLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            //LogCall<TRequest, TResponse>(MethodType.Unary, context);

            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                // Note: The gRPC framework also logs exceptions thrown by handlers to .NET Core logging.
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), ex, $"Error thrown by {context.Method}.");                

                throw;
            }
        }
       
    }
}