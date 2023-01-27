using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.Graph
{
    public class BaseRefLandmarksTool
    {
        private readonly Model _readModel;
        private readonly TraceModelBuilder _traceModelBuilder;

        public BaseRefLandmarksTool(Model readModel, TraceModelBuilder traceModelBuilder)
        {
            _readModel = readModel;
            _traceModelBuilder = traceModelBuilder;
        }

        public void ApplyTraceToBaseRef(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace,
            bool needToInsertLandmarksForEmptyNodes)
        {
            var traceModel = _readModel.GetTraceComponentsByIds(trace);
            var modelWithoutAdjustmentPoint = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            if (needToInsertLandmarksForEmptyNodes)
                InsertLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            SetLandmarksLocation(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            AddNamesAndTypesForLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);
        }

        public void SetLandmarksLocation(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 1; i < model.EquipArray.Length; i++)
            {
                if (landmarks[i].RelatedEventNumber != 0) continue; // landmark is associated with keyEvent and we can't move it

                var ratio = GetRatioBaseRefToGraphAroundEmptyNode(sorData, model, i);
                landmarks[i].Location = landmarks[i - 1].Location + sorData.GetOwtFromMm((int)(model.DistancesMm![i - 1] * ratio));
            }
        }

        private void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var newLandmarks = new OtdrDataFormat.Landmark[model.EquipArray.Length];

            var oldLandmarkIndex = 0;
            for (var i = 0; i < model.EquipArray.Length; i++)
            {
                if (model.EquipArray[i].Type > EquipmentType.CableReserve)
                {
                    newLandmarks[i] = sorData.LinkParameters.LandmarkBlocks[oldLandmarkIndex];
                    oldLandmarkIndex++;
                }
                else
                    newLandmarks[i] = new OtdrDataFormat.Landmark() { Code = LandmarkCode.Manhole };
            }

            sorData.LinkParameters.LandmarkBlocks = newLandmarks;
            sorData.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        public void AddNamesAndTypesForLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 0; i < landmarks.Length; i++)
            {
                var landmarkTitle = model.NodeArray[i].Title;
                if (i != 0 && !string.IsNullOrEmpty(model.EquipArray[i].Title))
                    landmarkTitle += $@" / {model.EquipArray[i].Title}";

                landmarks[i].Comment = landmarkTitle ?? ""; // utf8, Reflect can now read it
                landmarks[i].Code = model.EquipArray[i].Type.ToLandmarkCode();
                landmarks[i].GpsLatitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lat);
                landmarks[i].GpsLongitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lng);
            }
        }
        private double GetRatioBaseRefToGraphAroundEmptyNode(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model, int emptyNodeIndex)
        {
            var leftRealEquipmentIndex = emptyNodeIndex - 1;
            while (model.EquipArray[leftRealEquipmentIndex].Type <= EquipmentType.CableReserve) leftRealEquipmentIndex--;
            var rightRealEquipmentIndex = emptyNodeIndex + 1;
            while (model.EquipArray[rightRealEquipmentIndex].Type <= EquipmentType.CableReserve) rightRealEquipmentIndex++;

            var onGraph = GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            var onBaseRef =
                sorData.GetDistanceBetweenLandmarksInMm(leftRealEquipmentIndex, rightRealEquipmentIndex);
            return ((double)onBaseRef) / onGraph;
        }

        private int GetDistanceBetweenRealEquipmentsOnGraphMm(TraceModelForBaseRef model, int leftEquipmentIndex, int rightEquipmentIndex)
        {
            if (rightEquipmentIndex - leftEquipmentIndex == 1)
                return model.DistancesMm![leftEquipmentIndex];

            return model.DistancesMm![leftEquipmentIndex] +
                   GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftEquipmentIndex + 1, rightEquipmentIndex);
        }
    }
}