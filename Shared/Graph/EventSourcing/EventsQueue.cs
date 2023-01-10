using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.Graph;

public class EventsQueue
{
    public List<object> EventsWaitingForCommit { get; } = new List<object>();
    private readonly ILogger<EventsQueue> _logger;
    private readonly Model _writeModel;

    public EventsQueue(ILogger<EventsQueue> logger, Model writeModel)
    {
        _logger = logger;
        _writeModel = writeModel;
    }

    public string? Add(object e)
    {
        var result = _writeModel.Apply(e);
        if (result == null)
            EventsWaitingForCommit.Add(e);
        else
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), result);
        return result;

    }
    public void Commit()
    {
        EventsWaitingForCommit.Clear();
    }
}