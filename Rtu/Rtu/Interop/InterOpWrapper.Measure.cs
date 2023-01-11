using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

/// <summary>
/// For usual measurement with parameters from file, c++ code will allocate memory and put there results.
/// We should only pass empty pointer which will be filled out with address where we can get measurement result.
/// 
/// In all other cases when we have some data which should be transfered to c++ code to perform any actions upon it:
/// - base reflectogram;
/// - measurement to analysis;
/// such data should be serialized into byte[] and by CreateSorPtr passed to c++ code:
/// memory will be allocated, data copied and pointer will be returned - so we can use
/// this pointer for asking c++ code to perform some actions upon our data,
/// then again by this pointer we can get processed data back, calling GetSorDataSize and GetSorData,
/// and must free allocated memory calling FreeSorPtr
/// 
/// </summary>
public partial class InterOpWrapper
{
    // EXTERN_C __declspec(dllexport) int MeasPrepare(int mMode);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MeasPrepare")]
    public static extern int MeasPrepare(int measurementMode);

    // EXTERN_C __declspec(dllexport) int MeasStep(TSorData** rezSD);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MeasStep")]
    public static extern int MeasStep(ref IntPtr sorData);

    // EXTERN_C __declspec(dllexport) int MeasStop(TSorData** fullSD, int stopMode);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MeasStop")]
    public static extern int MeasStop(ref IntPtr sorData, int isImmediateStop);

    public int ConvertLmaxKmToNs()
    {
        string? lmaxString = GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveLmax);
        int lmax;
        if (!int.TryParse(lmaxString, out lmax))
            lmax = 200;
        string? riString = GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveRi);
        double ri;
        if (!double.TryParse(riString, out ri))
            ri = 1.47500;

        const double lightSpeed = 0.000299792458; // km/ns
        int lmaxNs = (int) (lmax * ri / lightSpeed);
        return lmaxNs;
    }

    public int ConvertLmaxOwtToNs(byte[] buffer)
    {
        const int owtsInTwoWayNs = 5;

        var str = SorData.TryGetFromBytes(buffer, out var sorData);
        if (!string.IsNullOrEmpty(str) || sorData == null)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), str);
            return -1;
        }
        int lmaxOwt = sorData.IitParameters.DistnaceRangeUser;
        if (lmaxOwt == -1)
            lmaxOwt = (int)sorData.FixedParameters.AcquisitionRange;

        return lmaxOwt / owtsInTwoWayNs;
    }

    public bool PrepareMeasurement(bool isAver)
    {
        var error = MeasPrepare(isAver ? 601 : 600);
        if (error != 0)
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Error {error} in MeasPrepare");
        return error == 0;
    }

    public int DoMeasurementStep(ref IntPtr sorData)
    {
        var result = MeasStep(ref sorData);
        return result;
    }

    public int StopMeasurement(bool isImmediateStop)
    {
        IntPtr sorData = IntPtr.Zero;
        return MeasStop(ref sorData, isImmediateStop ? 1 : 0);
    }
}