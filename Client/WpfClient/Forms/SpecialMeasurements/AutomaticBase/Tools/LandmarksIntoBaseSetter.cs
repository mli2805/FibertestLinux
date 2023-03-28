using System.Linq;
using Fibertest.Graph;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.WpfClient
{
    public class LandmarksIntoBaseSetter
    {
        private readonly Model _readModel;
        private readonly TraceModelBuilder _traceModelBuilder;

        public LandmarksIntoBaseSetter(Model readModel, TraceModelBuilder traceModelBuilder)
        {
            _readModel = readModel;
            _traceModelBuilder = traceModelBuilder;
        }

        public void ApplyTraceToAutoBaseRef(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var traceModel = _readModel.GetTraceComponentsByIds(trace);
            var model = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);

            var ratio = GetRatio(sorData, model);
            var newLandmarks = new OtdrDataFormat.Landmark[model.EquipArray.Length];

            for (int i = 0; i < model.EquipArray.Length; i++)
            {
                var newLandmark = new OtdrDataFormat.Landmark();
                newLandmark.Code = model.EquipArray[i].Type.ToLandmarkCode();
                var landmarkTitle = model.NodeArray[i].Title ?? "";
                if (i != 0 && !string.IsNullOrEmpty(model.EquipArray[i].Title))
                    landmarkTitle += $@" / {model.EquipArray[i].Title}";
                newLandmark.Comment = landmarkTitle;
                newLandmark.GpsLatitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lat);
                newLandmark.GpsLongitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lng);
                newLandmark.Location = i == 0 
                        ? 0 
                        : newLandmarks[i - 1].Location + 
                            sorData.GetOwtFromMm((int)(model.DistancesMm![i - 1] * ratio));
                newLandmarks[i] = newLandmark;
            }

            newLandmarks[0].RelatedEventNumber = 1;
            newLandmarks.Last().RelatedEventNumber = 2;

            sorData.LinkParameters.LandmarkBlocks = newLandmarks;
            sorData.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        private double GetRatio(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var onBaseRef = sorData.GetTraceLengthKm() * 1000000;
            var onGraph = model.DistancesMm!.Sum();
            return onBaseRef/onGraph;
        }
    }
}