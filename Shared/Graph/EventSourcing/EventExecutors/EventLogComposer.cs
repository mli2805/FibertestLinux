using Fibertest.Dto;
using NEventStore;

namespace Fibertest.Graph;

public class EventLogComposer
{
    private readonly Model _model;
    private readonly EventToLogLineParser _eventToLogLineParser;

    private int _ordinal = 1;

    public EventLogComposer(Model model, EventToLogLineParser eventToLogLineParser)
    {
        _model = model;
        _eventToLogLineParser = eventToLogLineParser;
    }

    public void Initialize()
    {
        _eventToLogLineParser.InitializeBySnapshot(_model);
        _ordinal = _model.UserActionsLog.Count + 1;
    }

    public void AddEventToLog(EventMessage msg)
    {
        try
        {
            var username = (string)msg.Headers[@"Username"];
            var user = _model.Users.FirstOrDefault(u => u.Title == username);

            var line = _eventToLogLineParser.ParseEventBody(msg.Body);
            // event should be parsed even before check in order to update internal dictionaries
            if (line == null || user == null
                             || user.Role < Role.Developer && line.OperationCode != LogOperationCode.EventsAndSorsRemoved)
                return;

            line.Ordinal = _ordinal;
            line.Username = username;
            line.ClientIp = (string)msg.Headers[@"ClientIp"];
            line.Timestamp = (DateTime)msg.Headers[@"Timestamp"];
            _model.UserActionsLog.Insert(0, line);
            _ordinal++;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}