﻿using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class OtdrManager
    {
        public ReturnCode MeasureWithBase(CancellationToken[] tokens, byte[] buffer, Charon? bopCharonToShowPortOnDisplay)
        {
            var result = ReturnCode.Error;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = _interOpWrapper.SetSorData(buffer);
            if (_interOpWrapper.SetMeasurementParametersFromSor(ref baseSorData))
            {
                _interOpWrapper.ForceLmaxNs(_interOpWrapper.ConvertLmaxOwtToNs(buffer));
                result = Measure(tokens, bopCharonToShowPortOnDisplay);
            }

            // free memory where was base sor data
            _interOpWrapper.FreeSorDataMemory(baseSorData);
            return result;
        }

        public ReturnCode DoManualMeasurement(CancellationToken[] tokens, bool shouldForceLmax, Charon? bopCharonToShowPortOnDisplay)
        {
            if (shouldForceLmax)
                _interOpWrapper.ForceLmaxNs(_interOpWrapper.ConvertLmaxKmToNs());

            return Measure(tokens, bopCharonToShowPortOnDisplay);
        }

        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private ReturnCode Measure(CancellationToken[] tokens, Charon? bopCharonToShowPortOnDisplay)
        {
            _logger.Info(Logs.RtuManager, "Measurement begin.");

            if (!_interOpWrapper.PrepareMeasurement(true))
            {
                _logger.Info(Logs.RtuManager, "Prepare measurement error!");
                return ReturnCode.MeasurementPreparationError;
            }

            bopCharonToShowPortOnDisplay?.ShowMessageMeasurementPort();

            if (!_interOpWrapper.SetTuningApdMode(1))
            {
                _logger.Info(Logs.RtuManager, "Prepare measurement error!");
                return ReturnCode.MeasurementPreparationError;
            }

            var result = MeasureSteps(tokens);

            bopCharonToShowPortOnDisplay?.ShowOnDisplayMessageReady();

            return result;
        }

        private ReturnCode MeasureSteps(CancellationToken[] tokens)
        {
            try
            {
                bool hasMoreSteps;
                int step = 0;
                do
                {
                    if (tokens.IsCancellationRequested())
                    {
                        _interOpWrapper.StopMeasurement(true);
                        _logger.Info(Logs.RtuManager, "Measurement interrupted between steps.");
                        return ReturnCode.MeasurementInterrupted;
                    }

                    var result = _interOpWrapper.DoMeasurementStep(ref _sorData);
                    var buffer = GetLastSorDataBuffer();
                    if (buffer == null)
                        return ReturnCode.MeasurementError;
                    _logger.Debug(Logs.RtuManager, $"  MeasStep #{++step} returned {buffer.Length} bytes");

                    if (result != 0 && result != 10001)
                    {
                        _logger.Info(Logs.RtuManager, $"  MeasStep returned {result}");
                        return ReturnCode.MeasurementError;
                    }
                    hasMoreSteps = result == 0;

                } while (hasMoreSteps);

            }
            catch (Exception e)
            {
                _logger.Info(Logs.RtuManager, e.Message);
                return ReturnCode.MeasurementError;
            }

            _logger.Info(Logs.RtuManager, "Measurement ended normally.");
            return ReturnCode.MeasurementEndedNormally;
        }

        public byte[]? GetLastSorDataBuffer()
        {
            int bufferLength = _interOpWrapper.GetSorDataSize(_sorData);
            if (bufferLength == -1)
            {
                _logger.Info(Logs.RtuManager, "  _sorData is null");
                return null;
            }
            byte[] buffer = new byte[bufferLength];

            var size = _interOpWrapper.GetSordata(_sorData, buffer, bufferLength);
            if (size == -1)
            {
                _logger.Info(Logs.RtuManager, "  Error in GetLastSorData");
                return null;
            }
            return buffer;
        }

        public byte[]? ApplyAutoAnalysis(byte[] measBytes)
        {
            var measIntPtr = _interOpWrapper.SetSorData(measBytes);
            _logger.Debug(Logs.RtuManager, "  SetSorData done.");

            if (!_interOpWrapper.MakeAutoAnalysis(ref measIntPtr))
            {
                _logger.Debug(Logs.RtuManager, "  ApplyAutoAnalysis error.");
                return null;
            }
            _logger.Debug(Logs.RtuManager, "  ApplyAutoAnalysis done.");
            var size = _interOpWrapper.GetSorDataSize(measIntPtr);
            _logger.Debug(Logs.RtuManager, "  GetSorDataSize done.");
            byte[] resultBytes = new byte[size];
            _interOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            _logger.Debug(Logs.RtuManager, "  GetSorData done.");
            _interOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public byte[]? Sf780_779(byte[] measBytes)
        {
            var measIntPtr = _interOpWrapper.SetSorData(measBytes);
            if (!_interOpWrapper.Analyze(ref measIntPtr, 1))
                return null;

            if (!_interOpWrapper.InsertIitEvents(ref measIntPtr))
                return null;

            var size = _interOpWrapper.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            _interOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            _interOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public byte[]? Sf780(byte[] measBytes)
        {
            var measIntPtr = _interOpWrapper.SetSorData(measBytes);
            if (!_interOpWrapper.Analyze(ref measIntPtr, 1))
                return null;

            var size = _interOpWrapper.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            _interOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            _interOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public OtdrDataKnownBlocks ApplyFilter(byte[] sorBytes, bool isFilterOn)
        {
            var sorData = SorData.FromBytes(sorBytes);
            sorData.IitParameters.Parameters = SetBitFlagInParameters(sorData.IitParameters.Parameters, IitBlockParameters.Filter, isFilterOn);
            return sorData;
        }

        public bool IsFilterOnInBase(byte[] sorBytes)
        {
            var sorData = SorData.FromBytes(sorBytes);
            return (sorData.IitParameters.Parameters & IitBlockParameters.Filter) != 0;
        }

        private IitBlockParameters SetBitFlagInParameters(IitBlockParameters parameters, IitBlockParameters parameter, bool flag)
        {
            return flag
                ? parameters | parameter
                : parameters & ~parameter;
        }
    }
}
