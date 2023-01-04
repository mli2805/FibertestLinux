using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter
{
    public interface IFtSignalRClient
    {
        Task<bool> IsSignalRConnected(bool isLog = true);
        Task NotifyAll(string eventType, string dataInJson);
        Task SendToOne(string connectionId, string eventType, string dataInJson);
        Task<bool> CheckServerIn();

        string ServerConnectionId { get; set; }
    }

    public class FtSignalRClient : IFtSignalRClient, IDisposable
    {
        private readonly ILogger<FtSignalRClient> _logger;
        private HubConnection? _connection;
        private readonly bool _isWebApiInstalled;
        private readonly string _webApiUrl;

        public string ServerConnectionId { get; set; } = string.Empty;

        public FtSignalRClient(ILogger<FtSignalRClient> logger, IOptions<DataCenterConfig> fullConfig)
        {
            _logger = logger;

            _isWebApiInstalled = fullConfig.Value.WebApi.BindingProtocol != "none";
            _webApiUrl = $"{fullConfig.Value.WebApi.BindingProtocol}://{fullConfig.Value.WebApi.DomainName}:{(int)TcpPorts.WebApiListenTo}/webApiSignalRHub";
        }

        private void Build()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_webApiUrl, (opts) =>
                {
                    opts.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // bypass SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (_, _, _, sslPolicyErrors) =>
                                {
                                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Negotiation with server returns sslPolicyErrors: {sslPolicyErrors}");
                                    return true;
                                };
                        return message;
                    };
                })
                .Build();

            _connection.Closed += async (error) =>
            {
                if (error != null)
                    _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"FtSignalRClient connection was closed: {error.Message}");
                //                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "FtSignalRClient connection was closed.");
                await Task.Delay(1);
            };

            _connection.On<string>("NotifyServer", connId =>
            {
                ServerConnectionId = connId;
                //                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"NotifyServer returned id {connId}");
            });
        }


        // DataCenter notifies WebClients
        public async Task NotifyAll(string eventType, string dataInJson)
        {
            if (!_isWebApiInstalled) return;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (_connection != null && isConnected)
                {
                    var unused = _connection.InvokeAsync("NotifyAll", eventType, dataInJson);
                    if (eventType != "NotifyMonitoringStep" && eventType != "NudgeSignalR") // too many
                        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"FtSignalRClient NotifyAll: {eventType} sent successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"FtSignalRClient NotifyAll: {eventType} " + ex.Message);
            }
        }

        // use it for ClientsMeasurement
        public async Task SendToOne(string connectionId, string eventType, string dataInJson)
        {
            if (!_isWebApiInstalled) return;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (_connection != null && isConnected)
                {
                    var unused = _connection.InvokeAsync("SendToOne", connectionId, eventType, dataInJson);
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"FtSignalRClient: {eventType} sent successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"FtSignalRClient: {eventType} " + ex.Message);
            }
        }

        public async Task<bool> CheckServerIn()
        {
            if (!_isWebApiInstalled) return true;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (_connection != null && isConnected)
                {
                    var unused = _connection.InvokeAsync("CheckServerIn");
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "Exception in FtSignalRClient CheckServerIn: " + e.Message);
            }
            return false;
        }

        public async Task<bool> IsSignalRConnected(bool isLog = true)
        {
            if (!_isWebApiInstalled) return false;
            if (_connection == null)
            {
                if (isLog) _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Build signalR connection to {_webApiUrl}");
                try
                {
                    Build();
                }
                catch (Exception e)
                {
                    if (isLog) _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"Build signalR connection: " + e.Message);
                    return false;
                }
                if (_connection != null && isLog) 
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"SignalR connection state is {_connection.State}");
                await Task.Delay(500);
            }

            if (_connection != null && _connection.State != HubConnectionState.Connected)
            {
                if (isLog) _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Start signalR connection to {_webApiUrl}");
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception e)
                {
                    if (isLog) _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"FtSignalRClient Start connection: " + e.Message);
                    _connection = null;
                    return false;
                }
                if (isLog) _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"SignalR connection state is {_connection.State}");
                await Task.Delay(500);
            }

            return true;
        }

        public async void Dispose()
        {
            if (_connection != null)
                await _connection.DisposeAsync();
        }
    }
}
