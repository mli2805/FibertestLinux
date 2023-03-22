using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;
using GMap.NET;

namespace Fibertest.Graph
{
    public class LandmarksBaseParser
    {
        private readonly Model _readModel;

        public LandmarksBaseParser(Model readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarks(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var nodesWithoutPoints = _readModel.GetTraceNodesExcludingAdjustmentPoints(trace.TraceId).ToList();

            var gpsDistance = 0.0;
            var result = new List<Landmark>();
            var linkParameters = sorData.LinkParameters;
            var prevLocation = GetPointLatLng(linkParameters.LandmarkBlocks[0]);
            var prevOwt = linkParameters.LandmarkBlocks[0].Location;
            for (int i = 0; i < linkParameters.LandmarksCount; i++)
            {
                var sorLandmark = linkParameters.LandmarkBlocks[i];
                var titles = sorLandmark.Comment.Split('/');
                var equipment = i > 0 ? _readModel.Equipments.First(e => e.EquipmentId == trace.EquipmentIds[i]) : null;
                var fiber = i > 0 ? _readModel.Fibers.First(f => f.FiberId == trace.FiberIds[i - 1]) : null;
                var section = fiber == null
                    ? 0
                    : fiber.UserInputedLength > 0
                        ? fiber.UserInputedLength / 1000
                        : GisLabCalculator.GetDistanceBetweenPointLatLng(prevLocation, GetPointLatLng(sorLandmark)) / 1000;
                gpsDistance += section;
                var comment = i == 0
                        ? _readModel.Rtus.First(r => r.NodeId == nodesWithoutPoints[i]).Comment
                        : _readModel.Nodes.First(n => n.NodeId == nodesWithoutPoints[i]).Comment;
                var landmark = new Landmark
                {
                    IsFromBase = true,
                    Number = i,
                    NodeId = nodesWithoutPoints[i],
                    NodeTitle = titles.Length > 0 ? titles[0].Trim() : "",
                    FiberId = fiber?.FiberId ?? Guid.Empty,
                    NodeComment = comment,
                    EquipmentId = trace.EquipmentIds[trace.NodeIds.IndexOf(nodesWithoutPoints[i])],
                    EquipmentTitle = titles.Length > 1 ? titles[1].Trim() : "",
                    EquipmentType = ToEquipmentType(sorLandmark.Code),
                    EventNumber = sorLandmark.RelatedEventNumber - 1,
                    LeftCableReserve = equipment?.CableReserveLeft ?? 0,
                    RightCableReserve = equipment?.CableReserveRight ?? 0,
                    GpsDistance = gpsDistance,
                    GpsSection = section,
                    IsUserInput = fiber == null ? false : fiber.UserInputedLength > 0,
                    OpticalDistance = sorData.OwtToLenKm(sorLandmark.Location),
                    OpticalSection = sorData.OwtToLenKm(sorLandmark.Location - prevOwt),
                    GpsCoors = GetPointLatLng(sorLandmark),
                };
                result.Add(landmark);
                prevLocation = landmark.GpsCoors;
                prevOwt = sorLandmark.Location;
            }
            return result;
        }

        private PointLatLng GetPointLatLng(OtdrDataFormat.Landmark landmark)
        {
            var lat = SorIntCoorToDoubleInDegrees(landmark.GpsLatitude);
            var lng = SorIntCoorToDoubleInDegrees(landmark.GpsLongitude);
            return new PointLatLng(lat, lng);
        }

        private double SorIntCoorToDoubleInDegrees(int coor)
        {
            var degrees = Math.Truncate(coor / 1e6);
            var minutes = Math.Truncate(coor % 1e6 / 1e4);
            var seconds = (coor % 1e4) / 100;
            return degrees + minutes / 60 + seconds / 3600;
        }

        private static EquipmentType ToEquipmentType(LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return EquipmentType.Rtu;
                case LandmarkCode.Coupler: return EquipmentType.Closure;
                case LandmarkCode.WiringCloset: return EquipmentType.Cross;
                case LandmarkCode.Manhole: return EquipmentType.EmptyNode;
                case LandmarkCode.CableSlackLoop: return EquipmentType.CableReserve;
                case LandmarkCode.RemoteTerminal: return EquipmentType.Terminal;
                case LandmarkCode.Other: return EquipmentType.Other;
            }
            return EquipmentType.Error;
        }

    }


}