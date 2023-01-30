using System.Collections.Generic;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient;

public class AllOpticalEventsReportProvider
{
    private readonly DataCenterConfig _server;
    private readonly CurrentUser _currentUser;
    private readonly CurrentGis _currentGis;
    private readonly Model _readModel;
    private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
    private readonly AccidentLineModelFactory _accidentLineModelFactory;
    private OpticalEventsReportModel _reportModel = null!;

    private List<OpticalEventModel> _events = null!;

    public AllOpticalEventsReportProvider(DataCenterConfig server, CurrentUser currentUser,
        CurrentGis currentGis, Model readModel,
        OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, AccidentLineModelFactory accidentLineModelFactory)
    {
        _server = server;
        _currentUser = currentUser;
        _currentGis = currentGis;
        _readModel = readModel;
        _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        _accidentLineModelFactory = accidentLineModelFactory;
    }

    public string? Create(OpticalEventsReportModel reportModel)
    {
        _reportModel = reportModel;
        return null;
    }
}