using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class OtdrManager
    {
      private MoniLevelType GetMoniLevelType(RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor:
                    return MoniLevelType.Minor;
                case RftsLevelType.Major:
                    return MoniLevelType.Major;
                case RftsLevelType.Critical:
                    return MoniLevelType.Critical;
                default:
                    return MoniLevelType.User;
            }
        }

        private void SetMoniResultFlags(MoniResult moniResult, ComparisonReturns returnCode)
        {
            switch (returnCode)
            {
                case ComparisonReturns.Ok:
                    break;
                case ComparisonReturns.FiberBreak:
                    moniResult.IsFiberBreak = true;
                    break;
                case ComparisonReturns.NoFiber:
                    moniResult.IsNoFiber = true;
                    break;
                default:
                    _logger.LLog(Logs.RtuManager.ToInt(), $"something goes wrong, code {returnCode}");
                    break;
            }
        }

        private static EmbeddedData BaseBufferToEmbeddedData(byte[] buffer)
        {
            return new EmbeddedData
            {
                Description = "SOR",
                DataSize = buffer.Length,
                Data = buffer.ToArray()
            };
        }

        public MoniResult CompareMeasureWithBase(byte[] baseBuffer, byte[] measBuffer, bool includeBase)
        {
            var baseSorData = SorData.FromBytes(baseBuffer);
            var measSorData = SorData.FromBytes(measBuffer);
            measSorData.IitParameters.Parameters = baseSorData.IitParameters.Parameters;

            var embeddedData = new List<EmbeddedData>();
            if (includeBase)
                embeddedData.Add(BaseBufferToEmbeddedData(baseBuffer));

            MoniResult moniResult = Compare(baseSorData, ref measSorData, embeddedData);

            measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
            measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;

            moniResult.Accidents = measSorData.GetAccidents();
            moniResult.SorBytes = measSorData.ToBytes();

            return moniResult;
        }

        private MoniResult Compare(OtdrDataKnownBlocks baseSorData, ref OtdrDataKnownBlocks cleanMeasSorData, List<EmbeddedData> rftsEventsList)
        {
            MoniResult moniResult = new MoniResult();

            var levelCount = baseSorData.RftsParameters.LevelsCount;
            _logger.LLog(Logs.RtuManager.ToInt(), $"Comparison begin. Level count = {levelCount}");

            for (int i = 0; i < levelCount; i++)
            {
                var rftsLevel = baseSorData.RftsParameters.Levels[i];
                if (rftsLevel.IsEnabled)
                {
                    baseSorData.RftsParameters.ActiveLevelIndex = i;

                    var measBytes = cleanMeasSorData.ToBytes();
                    var measSorData = SorData.FromBytes(measBytes);

                    CompareOneLevel(baseSorData, ref measSorData, GetMoniLevelType(rftsLevel.LevelName), moniResult);
                    rftsEventsList.Add(measSorData.RftsEventsToEmbeddedData());

                    if (i == levelCount - 1)
                        cleanMeasSorData = measSorData;
                }
            }

            return moniResult;
        }

        private void CompareOneLevel(OtdrDataKnownBlocks baseSorData, ref OtdrDataKnownBlocks measSorData,  MoniLevelType type, MoniResult moniResult)
        {
            try
            {
                var moniLevel = new MoniLevel {Type = type};

                // allocate memory
                var baseIntPtr = _interOpWrapper.SetSorData(baseSorData.ToBytes());
                _interOpWrapper.SetBaseForComparison(baseIntPtr);

                // allocate memory
                var measIntPtr = _interOpWrapper.SetSorData(measSorData.ToBytes());
                var returnCode = _interOpWrapper.CompareActiveLevel(measIntPtr);

                var size = _interOpWrapper.GetSorDataSize(measIntPtr);
                byte[] buffer = new byte[size];
                _interOpWrapper.GetSordata(measIntPtr, buffer, size);
                measSorData = SorData.FromBytes(buffer);

                moniLevel.IsLevelFailed = (measSorData.RftsEvents.Results & MonitoringResults.IsFailed) != 0;
                moniResult.Levels.Add(moniLevel);

                var levelResult = returnCode != ComparisonReturns.Ok ? returnCode.ToString() : moniLevel.IsLevelFailed ? "Failed!" : "OK!";
                _logger.LLog(Logs.RtuManager.ToInt(), $"Level {type} comparison result = {levelResult}!");

                SetMoniResultFlags(moniResult, returnCode);

                // free memory
                _interOpWrapper.FreeSorDataMemory(measIntPtr);
                _interOpWrapper.FreeSorDataMemory(baseIntPtr);
            }
            catch (Exception e)
            {
                _logger.LLog(Logs.RtuManager.ToInt(), $"Compare one level error: {e.Message}");
            }
        } }
}
