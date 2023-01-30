using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient;

public class ActualOpticalEventsReportProvider
{
    private readonly DataCenterConfig _server;
    private readonly Model _readModel;
    private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
    private readonly AccidentLineModelFactory _accidentLineModelFactory;
    private readonly CurrentGis _currentGis;
    private OpticalEventsReportModel _reportModel = null!;
    public ActualOpticalEventsReportProvider(DataCenterConfig server, 
        Model readModel, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, 
        AccidentLineModelFactory accidentLineModelFactory, CurrentGis currentGis)
    {
        _server = server;
        _readModel = readModel;
        _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        _accidentLineModelFactory = accidentLineModelFactory;
        _currentGis = currentGis;
    }

    public string? Create(OpticalEventsReportModel reportModel)
    {
        _reportModel = reportModel;

        return null;
    }
}