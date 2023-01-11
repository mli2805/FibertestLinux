using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph;

public class AccidentLineModelFactory
{
    private const string LeftArrow = "\U0001f860";
    private bool _isGisOn;
    private GpsInputMode _gpsInputMode;
    private bool _isDesktop;

    public AccidentLineModel Create(AccidentOnTraceV2 accident, int number,
        bool isGisOn, GpsInputMode gpsInputMode = GpsInputMode.DegreesMinutesAndSeconds, bool isDesktop = true)
    {
        _isGisOn = isGisOn;
        _gpsInputMode = gpsInputMode;
        _isDesktop = isDesktop;
        if (accident.IsAccidentInOldEvent)
        {
            return accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff
                ? CreateBadSegment(accident, number)
                : CreateInNode(accident, number);
        }
        else
            return CreateBetweenNodes(accident, number);
    }

    private AccidentLineModel CreateInNode(AccidentOnTraceV2 accidentInOldEvent, int number)
    {
        var model = new AccidentLineModel
        {
            Caption =
                $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                    accidentInOldEvent.OpticalTypeOfAccident.ToLetter()
                }) {Resources.SID_in_the_node}:",

            Number = number,
            AccidentSeriousness = accidentInOldEvent.AccidentSeriousness,
            AccidentTypeLetter = accidentInOldEvent.OpticalTypeOfAccident.ToLetter(),
            AccidentPlace = AccidentPlace.InNode,

            TopCenter = accidentInOldEvent.AccidentTitle,
            TopLeft = $@"RTU {LeftArrow} {accidentInOldEvent.AccidentToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
            Bottom2 = _isGisOn
                ? accidentInOldEvent.AccidentCoors.ToDetailedString(_gpsInputMode)
                : "",
            PngPath = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                ? BuildPath(@"FiberBrokenInNode.png")
                : accidentInOldEvent.IsAccidentInLastNode
                    ? BuildPath(@"AccidentInLastNode.png")
                    : BuildPath(@"AccidentInNode.png"),
            Position = accidentInOldEvent.AccidentCoors,
        };

        return model;
    }

    private AccidentLineModel CreateBetweenNodes(AccidentOnTraceV2 accidentAsNewEvent, int number)
    {
        var model = new AccidentLineModel
        {
            Caption =
                $@"{number}. {accidentAsNewEvent.AccidentSeriousness.ToLocalizedString()} ({
                    accidentAsNewEvent.OpticalTypeOfAccident.ToLetter()
                }) {Resources.SID_between_nodes}:",

            Number = number,
            AccidentSeriousness = accidentAsNewEvent.AccidentSeriousness,
            AccidentTypeLetter = accidentAsNewEvent.OpticalTypeOfAccident.ToLetter(),
            AccidentPlace = AccidentPlace.BetweenNodes,

            TopLeft = accidentAsNewEvent.Left!.Title,
            TopCenter = $@"RTU {LeftArrow} {accidentAsNewEvent.AccidentToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
            TopRight = accidentAsNewEvent.Right!.Title,
            Bottom1 = $@"{accidentAsNewEvent.AccidentToLeftOpticalDistanceKm:0.000} {Resources.SID_km}",
            Bottom2 = _isGisOn
                ? accidentAsNewEvent.AccidentCoors.ToDetailedString(_gpsInputMode)
                : "",
            Bottom3 = $@"{accidentAsNewEvent.AccidentToRightOpticalDistanceKm:0.000} {Resources.SID_km}",
            PngPath = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                ? BuildPath(@"FiberBrokenBetweenNodes.png")
                : BuildPath(@"AccidentBetweenNodes.png"),
            Position = accidentAsNewEvent.AccidentCoors
        };

        return model;
    }

    private AccidentLineModel CreateBadSegment(AccidentOnTraceV2 accidentInOldEvent, int number)
    {
        var model = new AccidentLineModel
        {
            Caption =
                $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {
                    Resources.SID_between_nodes
                }: ",

            Number = number,
            AccidentSeriousness = accidentInOldEvent.AccidentSeriousness,
            AccidentTypeLetter = @"C",
            AccidentPlace = AccidentPlace.BadSegment,

            TopLeft = accidentInOldEvent.Left!.Title,
            TopRight = accidentInOldEvent.Right!.Title,
            Bottom1 = $@"RTU {LeftArrow} {accidentInOldEvent.Left.ToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
            Bottom3 = $@"RTU {LeftArrow} {accidentInOldEvent.Right.ToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
            PngPath = BuildPath(@"BadSegment.png"),
            Position = accidentInOldEvent.Left.Coors,
        };

        return model;
    }

    private string BuildPath(string pngFile)
    {
        try
        {
            return _isDesktop
                ? $@"pack://application:,,,/Resources/AccidentSchemes/{pngFile}"
                : $@"./assets/AccidentSchemes/{pngFile}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }
}