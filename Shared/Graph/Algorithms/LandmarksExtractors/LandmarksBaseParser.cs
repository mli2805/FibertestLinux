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

            var result = new List<Landmark>();
            var linkParameters = sorData.LinkParameters;
            for (int i = 0; i < linkParameters.LandmarksCount; i++)
            {
                var sorLandmark = linkParameters.LandmarkBlocks[i];
                var titles = sorLandmark.Comment.Split('/');
                var comment = i == 0
                        ? _readModel.Rtus.First(r => r.NodeId == nodesWithoutPoints[i]).Comment
                        : _readModel.Nodes.First(n => n.NodeId == nodesWithoutPoints[i]).Comment;
                var landmark = new Landmark
                {
                    Number = i,
                    NodeId = nodesWithoutPoints[i],
                    NodeTitle = titles.Length > 0 ? titles[0].Trim() : "",
                    NodeComment = comment,
                    EquipmentId = trace.EquipmentIds[trace.NodeIds.IndexOf(nodesWithoutPoints[i])],
                    EquipmentTitle = titles.Length > 1 ? titles[1].Trim() : "",
                    EquipmentType = ToEquipmentType(sorLandmark.Code),
                    EventNumber = sorLandmark.RelatedEventNumber - 1,
                    Distance = sorData.OwtToLenKm(sorLandmark.Location),
                    GpsCoors = GetPointLatLng(sorLandmark),
                };
                result.Add(landmark);
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