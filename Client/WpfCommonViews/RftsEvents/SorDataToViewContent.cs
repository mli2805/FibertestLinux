using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace WpfCommonViews
{
    public class SorDataToViewContent
    {
        private readonly OtdrDataKnownBlocks _sorData;
        private readonly RftsEventsBlock _rftsEvents;
        private int _eventCount;

        private Dictionary<int, string> LineNameList => new Dictionary<int, string>
        {
            { 100, Resources.SID________Common_Information       },
            { 101, Resources.SID_Landmark_Name                   },
            { 102, Resources.SID_Landmark_Type                   },
            { 103, Resources.SID_State                           },
            { 104, Resources.SID_Damage_Type                     },
            { 105, Resources.SID_Distance__km                    },
            { 106, Resources.SID_Enabled                         },
            { 107, Resources.SID_Event_Type                      },
            { 200, Resources.SID________Current_Measurement      },
            { 201, Resources.SID_Reflectance_coefficient__dB     },
            { 202, Resources.SID_Attenuation_in_Closure__dB      },
            { 203, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 300, Resources.SID________Monitoring_Thresholds    },
            { 301, Resources.SID_Reflectance_coefficient__dB     },
            { 302, Resources.SID_Attenuation_in_Closure__dB      },
            { 303, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 400, Resources.SID________Deviations_from_Base     },
            { 401, Resources.SID_Reflectance_coefficient__dB     },
            { 402, Resources.SID_Attenuation_in_Closure__dB      },
            { 403, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 900, ""                                            },
        };

        public SorDataToViewContent(OtdrDataKnownBlocks sorData, RftsEventsBlock rftsEvents)
        {
            _sorData = sorData;
            _rftsEvents = rftsEvents;
        }

        public OneLevelTableContent Parse()
        {
            _eventCount = _rftsEvents.EventsCount;
            var eventContent = PrepareEmptyDictionary();

            ParseCommonInformation(eventContent);
            ParseCurrentMeasurement(eventContent.Table);
            ParseMonitoringThresholds(eventContent.Table);
            ParseDeviationFromBase(eventContent, _rftsEvents);

            return eventContent;
        }


        private OneLevelTableContent PrepareEmptyDictionary()
        {
            var eventsContent = new OneLevelTableContent();
            foreach (var pair in LineNameList)
            {
                var cells = new string[_eventCount + 1];
                cells[0] = pair.Value;
                eventsContent.Table.Add(pair.Key, cells);
            }
            return eventsContent;
        }

        private void ParseCommonInformation(OneLevelTableContent oneLevelTableContent)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                var landmark = _sorData.LinkParameters.LandmarkBlocks.FirstOrDefault(b => b.RelatedEventNumber == i + 1);
                if (landmark != null)
                {
                    oneLevelTableContent.Table[101][i + 1] = landmark.Comment;
                    oneLevelTableContent.Table[102][i + 1] = landmark.Code.ForTable();
                }

                oneLevelTableContent.Table[105][i + 1] = $@"{_sorData.OwtToLenKm(_sorData.KeyEvents.KeyEvents[i].EventPropagationTime):0.00000}";
                if ((_rftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                {
                    oneLevelTableContent.IsFailed = true;
                    if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                        oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][i + 1];
                    oneLevelTableContent.Table[105][i + 1] += Resources.SID___new_;
                }
                oneLevelTableContent.Table[106][i + 1] = _rftsEvents.Events[i].EventTypes.ForEnabledInTable();
                oneLevelTableContent.Table[107][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventCode.EventCodeForTable();
            }
        }

        private double AttenuationCoeffToDbKm(double p)
        {
            return p / _sorData.GetOwtToKmCoeff();
        }
        private void ParseCurrentMeasurement(Dictionary<int, string?[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[201][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventReflectance.ToString(CultureInfo.CurrentCulture);
                if (i == 0)
                    continue;
                var eventLoss = _sorData.KeyEvents.KeyEvents[i].EventLoss;
                var endOfFiberThreshold = _sorData.FixedParameters.EndOfFiberThreshold;
                eventTable[202][i + 1] = eventLoss > endOfFiberThreshold ? $@">{endOfFiberThreshold:0.000}" : $@"{eventLoss:0.000}";
                eventTable[203][i + 1] = $@"{AttenuationCoeffToDbKm(_sorData.KeyEvents.KeyEvents[i].LeadInFiberAttenuationCoefficient): 0.000}";
            }
        }

        private void ParseMonitoringThresholds(Dictionary<int, string?[]> eventTable)
        {
            var level = _sorData.RftsParameters.Levels.First(l => l.LevelName == _rftsEvents.LevelName);

            for (int i = 1; i < _eventCount; i++)
            {
                if ((_rftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                {
                    eventTable[301][i + 1] = _sorData.RftsParameters.UniversalParameters[5].ForTable();
                    eventTable[302][i + 1] = _sorData.RftsParameters.UniversalParameters[4].ForTable();

                    continue;
                }
                eventTable[301][i + 1] = level.ThresholdSets[i].ReflectanceThreshold.ForTable();
                eventTable[302][i + 1] = level.ThresholdSets[i].AttenuationThreshold.ForTable();
                eventTable[303][i + 1] = level.ThresholdSets[i].AttenuationCoefThreshold.ForTable();
            }
        }

        private void ParseDeviationFromBase(OneLevelTableContent oneLevelTableContent, RftsEventsBlock rftsEvents)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                if ((rftsEvents.Events[i].EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                {
                    oneLevelTableContent.IsFailed = true;
                    oneLevelTableContent.Table[104][i + 1] = @"B";
                    if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                        oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][i + 1];
                }

                if ((i == 0) || (_rftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                {

                }
                else
                {
                    oneLevelTableContent.Table[401][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].ReflectanceThreshold, i + 1, @"R");
                    if (i < _eventCount - 1)
                        oneLevelTableContent.Table[402][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].AttenuationThreshold, i + 1, @"L");
                    oneLevelTableContent.Table[403][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].AttenuationCoefThreshold, i + 1, @"C");

                }

                oneLevelTableContent.Table[103][i + 1] = 
                    _rftsEvents.Events[i].EventTypes.ForStateInTable(oneLevelTableContent.Table[104][i + 1] != null);
            }
        }

        private string ForDeviationInTable(OneLevelTableContent oneLevelTableContent, ShortDeviation deviation, int column, string letter)
        {
            var formattedValue = $@"{(short)deviation.Deviation / 1000.0: 0.000}";
            if ((deviation.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                formattedValue += $@" ( {letter} ) ";
                oneLevelTableContent.Table[104][column] += $@" {letter}";
                oneLevelTableContent.IsFailed = true;
                if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                    oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][column];
            }
            return formattedValue;
        }

    }
}