using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace GrpsClientLib;

public class Class2
{
    private readonly ILogger<Class2> _logger;

    public Class2(ILogger<Class2> logger)
    {
        _logger = logger;
    }

    public int GetInt()
    {
        _logger.Log(LogLevel.Information, Logs.Client.ToInt(), "GetInt in Class2");
        return 4;
    }
}