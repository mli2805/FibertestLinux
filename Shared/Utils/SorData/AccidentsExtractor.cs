using Fibertest.Dto;
using Fibertest.OtdrDataFormat;

namespace Fibertest.Utils
{
    public static class AccidentsExtractor
    {
        public static List<AccidentInSor> GetAccidents(this OtdrDataKnownBlocks sorData)
        {
            var result = new List<AccidentInSor>();
            var rftsEventsBlocks = sorData.GetRftsEventsBlockForEveryLevel().OrderByDescending(r => r.LevelName).ToList();
            foreach (var rftsEventsBlock in rftsEventsBlocks)
            {
                if ((rftsEventsBlock.Results & MonitoringResults.IsFailed) != 0)
                {
                    foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(rftsEventsBlock))
                    {
                        if (!IsDuplicate(result, accidentOnTrace))
                            result.Add(accidentOnTrace);
                    }
                }
            }

            return result;
        }

        private static bool IsDuplicate(List<AccidentInSor> alreadyFound, AccidentInSor accidentInSor)
        {
            if (alreadyFound.Any(a => a.BrokenRftsEventNumber == accidentInSor.BrokenRftsEventNumber &&
                                      a.OpticalTypeOfAccident == OpticalAccidentType.Break))
                return true;

            if (accidentInSor.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff &&
                alreadyFound.Any(a => a.BrokenRftsEventNumber == accidentInSor.BrokenRftsEventNumber &&
                                      a.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff))
                return true;

            if ((accidentInSor.OpticalTypeOfAccident == OpticalAccidentType.Reflectance || accidentInSor.OpticalTypeOfAccident == OpticalAccidentType.Loss) &&
                alreadyFound.Any(a => a.BrokenRftsEventNumber == accidentInSor.BrokenRftsEventNumber &&
                                      (a.OpticalTypeOfAccident == OpticalAccidentType.Reflectance || a.OpticalTypeOfAccident == OpticalAccidentType.Loss)))
                return true;

            return false;
        }

        private static IEnumerable<AccidentInSor> GetAccidentsForLevel(this OtdrDataKnownBlocks sorData, RftsEventsBlock rftsEventsBlock)
        {
            for (int keyEventIndex = 1; keyEventIndex < rftsEventsBlock.EventsCount; keyEventIndex++) // 0 - RTU
            {
                var rftsEvent = rftsEventsBlock.Events[keyEventIndex];

                if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                    yield return sorData.BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName);
                if ((rftsEvent.EventTypes & RftsEventTypes.IsFailed) != 0 || (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                    foreach (var opticalAccidentType in rftsEvent.GetOpticalTypesOfAccident())
                    {
                        var accident = sorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex) == -1
                                        ? sorData.BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName)
                                        : sorData.BuildAccidentInOldEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName);
                        accident.OpticalTypeOfAccident = opticalAccidentType;
                        yield return accident;
                    }
            }
        }

        private static AccidentInSor BuildAccidentInOldEvent(this OtdrDataKnownBlocks sorData,
            RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var accidentInSor = rftsEvent.GetOpticalTypeOfAccident() == OpticalAccidentType.LossCoeff
                ? new AccidentInSor
                {
                    IsAccidentInOldEvent = true,
                    BrokenRftsEventNumber = keyEventIndex,
                    AccidentSeriousness = level.ConvertToFiberState(),
                }

                : new AccidentInSor
                {
                    IsAccidentInOldEvent = true,
                    BrokenRftsEventNumber = keyEventIndex + 1,
                    AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                };

            accidentInSor.AccidentToRtuOpticalDistanceKm = sorData.KeyEventDistanceKm(keyEventIndex);
            accidentInSor.OpticalTypeOfAccident = rftsEvent.GetOpticalTypeOfAccident();
            accidentInSor.EventCode = sorData.KeyEvents.KeyEvents[keyEventIndex].EventCode.EventCodeForTable();
            accidentInSor.DeltaLen = sorData.GetDeltaLen(accidentInSor.EventCode[0]);
            return accidentInSor;
        }

        private static AccidentInSor BuildAccidentAsNewEvent(this OtdrDataKnownBlocks sorData, 
            RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var accidentAsNewEvent = new AccidentInSor()
            {
                IsAccidentInOldEvent = false,
                BrokenRftsEventNumber = keyEventIndex + 1, // keyEventIndex - index, keyEventIndex+1 number
                AccidentToRtuOpticalDistanceKm = sorData.KeyEventDistanceKm(keyEventIndex),
                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = rftsEvent.GetOpticalTypeOfAccident(),
                EventCode = sorData.KeyEvents.KeyEvents[keyEventIndex].EventCode.EventCodeForTable(),
            };
            accidentAsNewEvent.DeltaLen = sorData.GetDeltaLen(accidentAsNewEvent.EventCode[0]);
            return accidentAsNewEvent;
        }

    }
}
