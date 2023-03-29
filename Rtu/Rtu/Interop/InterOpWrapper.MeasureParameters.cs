using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    private string? GetLineOfVariantsForParam(ServiceFunctionFirstParam param)
    {
        int cmd = (int)ServiceFunctionCommand.GetParam;
        int prm1 = (int)param;

        IntPtr unmanagedPointer = IntPtr.Zero;
        int res = CppImportDecl.ServiceFunction(cmd, ref prm1, ref unmanagedPointer);
        if (res != 0)
            return null;

        return Marshal.PtrToStringAnsi(unmanagedPointer);
    }

    private string[] GetArrayOfVariantsForParam(ServiceFunctionFirstParam paramCode)
    {
        string? value = GetLineOfVariantsForParam(paramCode);
        if (value == null)
            return Array.Empty<string>();

        // if there is only one variant it will be returned without leading slash
        if (value[0] != '/')
            return new[] { value };

        var strs = value.Split('/');
        return strs.Skip(1).ToArray();
    }

    private bool SetParam(ServiceFunctionFirstParam param, int indexInLine)
    {
        try
        {
            int cmd = (int)ServiceFunctionCommand.SetParam;
            int prm1 = (int)param;
            IntPtr prm2 = new IntPtr(indexInLine);
            var result = CppImportDecl.ServiceFunction(cmd, ref prm1, ref prm2);
            if (result != 0)
            {
                _logger.Error(Logs.RtuManager, $"Set parameter error={result}!");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Set parameter: {e.Message}!");
            return false;
        }
    }

    public bool SetTuningApdMode(int mode)
    {
        int cmd = (int)ServiceFunctionCommand.SetParam;
        int prm1 = (int)ServiceFunctionFirstParam.TuningApdMode;
        IntPtr prm2 = new IntPtr(mode);
        var result = CppImportDecl.ServiceFunction(cmd, ref prm1, ref prm2);
        if (result != 0)
        {
            _logger.Error(Logs.RtuManager, $"Set TuningAPDMode error={result}!");
            return false;
        }
        return true;
    }

    public bool SetMeasurementParametersFromSor(ref IntPtr baseSorData)
    {
        int cmd = (int)ServiceFunctionCommand.SetParamFromSor;
        int reserved = 0;

        var result = CppImportDecl.ServiceFunction(cmd, ref reserved, ref baseSorData);
        if (result != 0)
            _logger.Error(Logs.RtuManager, $"Set parameters from sor error={result}!");
        return result == 0;
    }

    public bool ForceLmaxNs(int lmaxNs)
    {
        int cmd = (int)ServiceFunctionCommand.ParamMeasLmaxSet;
        IntPtr reserved = IntPtr.Zero;
        var result = CppImportDecl.ServiceFunction(cmd, ref lmaxNs, ref reserved);
        if (result != 1)
            _logger.Error(Logs.RtuManager, $"Force Lmax {lmaxNs} ns: Error = {result}!");
        else
            _logger.Info(Logs.RtuManager, $"Force Lmax {lmaxNs} ns: Ok");
        return result == 1;
    }
}