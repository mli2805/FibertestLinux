using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.Graph
{
    /// <summary>
    /// report for Debug
    /// prepare list of strings for log file
    /// </summary>
    public class SorDataParsingReporter
    {
        private OtdrDataKnownBlocks _sorData;
        private OtdrDataKnownBlocks _baseSorData;
        private readonly List<string> _report = new List<string>();

        public void DoReport(OtdrDataKnownBlocks sorData)
        {
            _sorData = sorData;
            _baseSorData = sorData.GetBase();

            var rftsEventsBlocks = _sorData.GetRftsEventsBlockForEveryLevel().ToList();
            ReportBaseAndMeasEventsParsing(rftsEventsBlocks);
        }

        private void ReportBaseAndMeasEventsParsing(List<RftsEventsBlock> levels)
        {
            _report.Add(@"   Base sor ");
            ReportLandmarks(_baseSorData);
            ReportKeyAndRftsEvents(_baseSorData, null);

            _report.Add("");
            _report.Add(@"   Measurement sor ");
            ReportLandmarks(_sorData);
            ReportKeyAndRftsEvents(_sorData, levels);

            var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var filename = Guid.NewGuid() + @".txt";
            var fullFilename = Path.Combine(tempFolder, filename);
            File.WriteAllLines(fullFilename, _report);
        }

        private void ReportLandmarks(OtdrDataKnownBlocks sorData)
        {
            _report.Add("");
            _report.Add(@"   Landmarks");
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                var lm = sorData.LinkParameters.LandmarkBlocks[i];
                var line = $@"index {i}  owt {lm.Location:D6} ({sorData.OwtToLenKm(lm.Location),7:F3} km)";
                if (lm.RelatedEventNumber != 0)
                    line = line + $@"  related event Number {lm.RelatedEventNumber}";
                _report.Add(line);
            }
        }

        private void ReportKeyAndRftsEvents(OtdrDataKnownBlocks sorData, List<RftsEventsBlock> rftsEventsBlocks)
        {
            var dict = new Dictionary<string, List<string>>();
            if (rftsEventsBlocks != null) // base
                foreach (var rftsEventsForOneLevel in rftsEventsBlocks)
                {
                    var list = rftsEventsForOneLevel.Events.Select(rftsEvent => EventStateToString(rftsEvent.EventTypes)).ToList();
                    dict.Add(rftsEventsForOneLevel.LevelName.ToString(), list);
                }

            _report.Add("");
            _report.Add(@"    Events (Minor / Major / Critical)");
            for (var i = 0; i < sorData.KeyEvents.KeyEvents.Length; i++)
            {
                var keyEvent = sorData.KeyEvents.KeyEvents[i];
                var line = $@"Number {i + 1}  owt {keyEvent.EventPropagationTime:D6} ({sorData.KeyEventDistanceKm(i),7:F3} km)";
                if (dict.ContainsKey(@"Minor")) line = line + @"   " + $@"{dict[@"Minor"][i],21}";
                if (dict.ContainsKey(@"Major")) line = line + @"   " + $@"{dict[@"Major"][i],21}";
                if (dict.ContainsKey(@"Critical")) line = line + @"   " + $@"{dict[@"Critical"][i],21}";
                _report.Add(line);
            }
            _report.Add("");
        }

        private string EventStateToString(RftsEventTypes state)
        {
            var result = "";
            if ((state & RftsEventTypes.IsNew) != 0) result = result + @" IsNew";
            if ((state & RftsEventTypes.IsFailed) != 0) result = result + @" IsFailed";
            if ((state & RftsEventTypes.IsFiberBreak) != 0) result = result + @" IsFiberBreak";
            if (result == "") result = @" IsMonitored";
            return result;
        }
    }
}