using Fibertest.Utils;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fibertest.DataCenter.Start;

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