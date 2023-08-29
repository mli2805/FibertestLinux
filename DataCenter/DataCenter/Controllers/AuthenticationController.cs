using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly Web2DCommandsProcessor _web2DCommandsProcessor;
    private readonly string _localIpAddress;
    private readonly string _version;

    public AuthenticationController(IWritableConfig<DataCenterConfig> config,
        ILogger<AuthenticationController> logger, Web2DCommandsProcessor web2DCommandsProcessor)
    {
        _logger = logger;
        _web2DCommandsProcessor = web2DCommandsProcessor;
        _localIpAddress = config.Value.General.ServerDoubleAddress.Main.Ip4Address;
        _version = config.Value.General.DatacenterVersion;
    }

    [Authorize]
    [HttpPost("ChangeConnectionId")]
    public async Task ChangeGuidWithSignalrConnectionId()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }
        dynamic user = JObject.Parse(body);
        var oldGuid = (string)user.oldGuid;
        var connId = (string)user.connId;
        await _web2DCommandsProcessor
            .ChangeGuidWithSignalrConnectionId(oldGuid, connId);
        _logger.Info(Logs.WebApi, $"User changed connection id.");
        Response.StatusCode = 201;
    }

    [Authorize]
    [HttpPost("Logout")]
    public async Task Logout()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }
        dynamic dto = JObject.Parse(body);
        var username = (string)dto.username;
        var connectionId = (string)dto.connectionId;
        await _web2DCommandsProcessor.UnRegisterClientAsync(
                new UnRegisterClientDto(username) { ClientConnectionId = connectionId });
        _logger.Info(Logs.WebApi, $"User {username} logged out.");
        Response.StatusCode = 201;
    }

    [Authorize]
    [HttpGet("Heartbeat/{connectionId}")]
    public async Task<RequestAnswer> Heartbeat(string connectionId)
    {
        var result = await _web2DCommandsProcessor
            .RegisterHeartbeat(new ClientHeartbeatDto(){ClientConnectionId = connectionId});
        return result;
    }

    [HttpPost("Login")]
    public async Task Login()
    {
        var clientIp = HttpContext.GetRemoteAddress(_localIpAddress);
        _logger.Info(Logs.WebApi, $"Authentication request from {clientIp}");
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }
        dynamic user = JObject.Parse(body);

        // if (user.version != _version)
        // {
        //     _logger.Info(Logs.WebApi, $"Web client version is {user.version}, Web API version is {_version}");
        //     await ReturnError(409, ReturnCode.VersionsDoNotMatch, "");
        //     return;
        // }

        var connectionId = Guid.NewGuid().ToString();
        var clientRegisteredDto = await _web2DCommandsProcessor
            .RegisterClientAsync(
                new RegisterClientDto((string)user.username, ((string)user.password).GetHashString()!)
                {
                    Addresses = new DoubleAddress()
                    {
                        Main = new NetAddress() { Ip4Address = clientIp, Port = 11080 },
                        HasReserveAddress = false
                    },
                    ClientConnectionId = connectionId,
                    IsUnderSuperClient = false,
                    IsWebClient = true,
                });

        _logger.Info(Logs.WebApi, $"Authentication response: {clientRegisteredDto.ReturnCode.GetLocalizedString()}");
        if (clientRegisteredDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
        {
            await ReturnError(401, clientRegisteredDto.ReturnCode, clientRegisteredDto.ErrorMessage!);
            return;
        }

        var identity = GetIdentity((string)user.username, clientRegisteredDto.Role);

        var now = DateTime.UtcNow;
        // create JWT
        var jwt = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                audience: AuthOptions.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromDays(AuthOptions.Lifetime)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new
        {
            username = (string)user.username,
            role = clientRegisteredDto.Role,
            zone = clientRegisteredDto.ZoneTitle,
            connectionId,
            jsonWebToken = encodedJwt,
            serverVersion = _version,
        };

        // response serialization
        Response.ContentType = "application/json";
        await Response.WriteAsync(JsonConvert.SerializeObject(response,
            new JsonSerializerSettings { Formatting = Formatting.Indented }));
    }

    private async Task ReturnError(int httpStatusCode, ReturnCode returnCode, string exceptionMessage)
    {
        Response.StatusCode = httpStatusCode;
        var responseError = new { returnCode, exceptionMessage, serverVersion = _version };
        Response.ContentType = "application/json";
        await Response.WriteAsync(JsonConvert.SerializeObject(responseError,
            new JsonSerializerSettings { Formatting = Formatting.Indented }));
    }

    private ClaimsIdentity GetIdentity(string username, Role role)
    {
        _logger.Info(Logs.WebApi, $"User {username.ToUpper()} is logged in");

        var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString()),
            };
        ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
        return claimsIdentity;

    }

}

public class AuthOptions
{
    public const string Issuer = "MyAuthServer"; // издатель токена
    public const string Audience = "http://localhost:4200/"; // потребитель токена ????
    const string Key = "100TimesMoreSecret_SecretKey_С_русскими_буквами!";   // ключ для шифрации
    public const int Lifetime = 400; // время жизни токена - дней
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}