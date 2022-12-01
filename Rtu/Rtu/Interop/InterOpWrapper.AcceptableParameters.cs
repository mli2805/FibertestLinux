using System.Globalization;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    public TreeOfAcceptableMeasParams? GetTreeOfAcceptableMeasParams()
    {
        var result = new TreeOfAcceptableMeasParams();
        var units = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Unit);
        if (units == null) return null;
        for (int i = 0; i < units.Length; i++)
        {
            SetParam(ServiceFunctionFirstParam.Unit, i);
            result.Units.Add(units[i], GetBranchOfAcceptableMeasParams());
        }
        return result;
    }

    private BranchOfAcceptableMeasParams GetBranchOfAcceptableMeasParams()
    {
        var result = new BranchOfAcceptableMeasParams();

        var bcStr = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Bc);
        result.BackscatteredCoefficient = bcStr == null ? 0 : double.Parse(bcStr[0], new CultureInfo("en-US"));
        var riStr = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Ri);
        result.RefractiveIndex = riStr == null ? 0 : double.Parse(riStr[0], new CultureInfo("en-US"));

        var distances = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Lmax);
        if (distances != null)
            for (int i = 0; i < distances.Length; i++)
            {
                SetParam(ServiceFunctionFirstParam.Lmax, i);
                result.Distances.Add(distances[i], GetLeafOfAcceptableMeasParams());
            }
        return result;
    }

    private LeafOfAcceptableMeasParams GetLeafOfAcceptableMeasParams()
    {
        var result = new LeafOfAcceptableMeasParams();
        result.Resolutions = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Res);
        result.PulseDurations = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Pulse);

        SetParam(ServiceFunctionFirstParam.IsTime, 1);
        result.PeriodsToAverage = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Time);

        SetParam(ServiceFunctionFirstParam.IsTime, 0);
        result.MeasCountsToAverage = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Navr);
        return result;
    }

    // Measurement Client
    public bool SetMeasParamsByPosition(List<MeasParamByPosition> list)
    {
        foreach (var measParam in list)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"{measParam.Param} - {measParam.Position}", 0, 3);
            if (!SetParam(measParam.Param, measParam.Position))
                return false;
            Thread.Sleep(200);
        }
        return true;
    }

    public List<MeasParamByPosition>? ValuesToPositions(List<MeasParamByPosition> allParams, VeexMeasOtdrParameters measParams,
        TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
    {
        var result = new List<MeasParamByPosition>(){
            allParams.First(p=>p.Param == ServiceFunctionFirstParam.Unit),
            allParams.First(p=>p.Param == ServiceFunctionFirstParam.Bc),
            allParams.First(p=>p.Param == ServiceFunctionFirstParam.Ri),
            new(){Param = ServiceFunctionFirstParam.IsTime, Position = 1 },
        };

        var unit = treeOfAcceptableMeasParams.Units.Values.ToArray()[0];

        if (measParams.distanceRange == null) return null;

        var lmaxStr = measParams.distanceRange;
        result.Add(new MeasParamByPosition()
        {
            Param = ServiceFunctionFirstParam.Lmax,
            Position = unit.Distances.Keys.ToList().IndexOf(lmaxStr)
        });
        var leaf = unit.Distances[lmaxStr];

        if (leaf.Resolutions == null || leaf.PeriodsToAverage == null || leaf.PulseDurations == null) return null;

        result.Add(new MeasParamByPosition
        {
            Param = ServiceFunctionFirstParam.Res,
            Position = Array.IndexOf(leaf.Resolutions, measParams.resolution)
        });
        result.Add(new MeasParamByPosition
        {
            Param = ServiceFunctionFirstParam.Pulse,
            Position = Array.IndexOf(leaf.PulseDurations, measParams.pulseDuration)
        });
        result.Add(new MeasParamByPosition
        {
            Param = ServiceFunctionFirstParam.Time,
            Position = Array.IndexOf(leaf.PeriodsToAverage, measParams.averagingTime)
        });

        return result;
    }
}