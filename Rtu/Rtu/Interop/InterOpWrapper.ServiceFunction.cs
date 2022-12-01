using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class InterOpWrapper
    {
        [DllImport("OtdrMeasEngine/iit_otdr.so")]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);

        public string? GetOtdrInfo(GetOtdrInfo infoType)
        {
            int cmd = (int)ServiceFunctionCommand.GetOtdrInfo;
            int prm = (int)infoType;
            IntPtr otdrInfo = IntPtr.Zero;

            var result = ServiceFunction(cmd, ref prm, ref otdrInfo);
            if (result == 0)
                return Marshal.PtrToStringAnsi(otdrInfo);

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Get OTDR info error ={result}!");
            return "";
        }

                public void SetEqualStepsOfMeasurement()
        {
            int cmd = (int)ServiceFunctionCommand.SetMeasSteppingMode;
            int prm = 1; // 1 - equal steps, 0 - permanently increasing
            IntPtr prm2 = IntPtr.Zero;

            ServiceFunction(cmd, ref prm, ref prm2);
        }

        public bool SetBaseForComparison(IntPtr baseSorData)
        {
            int cmd = (int)ServiceFunctionCommand.SetBase;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref baseSorData);
            if (result != 0)
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Set base for comparison error={result}!");
            return result == 0;
        }

        public ComparisonReturns CompareActiveLevel(IntPtr measSorData)
        {
            int cmd = (int)ServiceFunctionCommand.MonitorEvents;
            int includeBase = 0;

            var result = (ComparisonReturns)ServiceFunction(cmd, ref includeBase, ref measSorData);
            return result;
        }

        public bool MakeAutoAnalysis(ref IntPtr sorData)
        {
            int cmd = (int)ServiceFunctionCommand.Auto;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref sorData);
            if (result != 0)
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"MakeAutoAnalysis error={result}!");
            return result == 0;
        }

        // analysisMode = 0 - полный анализ; 1 - полуавтомат
        public bool Analyze(ref IntPtr sorData, int analysisMode)
        {
            int cmd = (int)ServiceFunctionCommand.Auto2;
            int mode = analysisMode;

            var result = ServiceFunction(cmd, ref mode, ref sorData);
            if (result != 0)
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Analyze error={result}!");
            return result == 0;
        }

        // установка участка - он же воротики - для обнаружения новых событий
        public bool InsertIitEvents(ref IntPtr sorData)
        {
            int cmd = (int)ServiceFunctionCommand.InsertIitEvents;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref sorData);
            if (result != 0)
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"InsertIitEvents error={result}!");
            return result == 0;
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.structuretoptr?view=netframework-4.0
        public double GetLinkCharacteristics(out ConnectionParams cp)
        {
            int cmd = (int)ServiceFunctionCommand.MeasConnParamsAndLmax; //749
            int prm1 = 1; // laserUnitIndex + 1;

            IntPtr prm2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ConnectionParams)));

            try
            {
                var result = ServiceFunction(cmd, ref prm1, ref prm2);
                if (result != 1)
                {
                    _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"GetLinkCharacteristics error={result}!");
                    cp = new ConnectionParams();
                    return -1;
                }

                var ccc = (ConnectionParams?)Marshal.PtrToStructure(prm2, typeof(ConnectionParams));
                if (ccc == null)
                {
                    cp = new ConnectionParams();
                    return -1;
                }

                cp = (ConnectionParams)ccc;

                const double lightSpeed = 0.000299792458; // km/ns
                var res = prm1 * lightSpeed / 1.4682;

                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Link characteristics:  Prm1 = {prm1} ns =>  Lmax {res:F} km");
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),
                    $"reflectance {cp.reflectance} dB, splice {cp.splice} dB, snr {cp.snr_almax}");
                return res;
            }
            finally
            {
                Marshal.FreeHGlobal(prm2);
            }
        }

    }
}
