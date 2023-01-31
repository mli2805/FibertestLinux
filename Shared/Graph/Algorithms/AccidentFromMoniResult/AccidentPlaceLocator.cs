using Fibertest.Dto;
using Fibertest.Utils;
using GMap.NET;
using Microsoft.Extensions.Logging;

namespace Fibertest.Graph
{
    public class AccidentPlaceLocator
    {
        private readonly ILogger<AccidentPlaceLocator> _logger;
        private readonly Model _model;

        public AccidentPlaceLocator(ILogger<AccidentPlaceLocator> logger, Model model)
        {
            _logger = logger;
            _model = model;
        }

        public void SetAccidentInNewEventGps(AccidentOnTraceV2 accident, Guid traceId)
        {
            var trace = _model.Traces.First(t => t.TraceId == traceId);

            var distances = GetGpsDistancesOfSegmentsBetweenLandmarks(accident, trace, out Node? leftNodeVm, out Node? rightNodeVm);
            if (distances == null) return;
            GetCableReserves(accident, traceId, out double leftReserveM, out double rightReserveM);
            var distanceBetweenTwoNodesOnGraphM = distances.Sum();

            var opticalLengthM = (accident.Right!.ToRtuOpticalDistanceKm - accident.Left!.ToRtuOpticalDistanceKm) * 1000;
            var coeff = opticalLengthM / (distanceBetweenTwoNodesOnGraphM + leftReserveM + rightReserveM);

            var distanceToAccidentOnGraphM = (accident.AccidentToRtuOpticalDistanceKm - accident.Left.ToRtuOpticalDistanceKm) * 1000 / coeff;

            if (distanceToAccidentOnGraphM <= leftReserveM)
            {
                accident.AccidentCoors = leftNodeVm!.Position;
                return;
            }

            if (distanceToAccidentOnGraphM > leftReserveM + distanceBetweenTwoNodesOnGraphM)
            {
                accident.AccidentCoors = rightNodeVm!.Position;
                return;
            }

            var segmentIndex = -1;
            var distancesSum = leftReserveM;
            while (distancesSum < distanceToAccidentOnGraphM)
            {
                segmentIndex++;
                distancesSum = distancesSum + distances[segmentIndex];
            }

            accident.AccidentCoors = GetPointOnBrokenSegment(trace,
                (distances[segmentIndex] - (distancesSum - distanceToAccidentOnGraphM)) / distances[segmentIndex],
                trace.NodeIds.IndexOf(leftNodeVm!.NodeId) + segmentIndex);
        }

        private PointLatLng GetPointOnBrokenSegment(Trace trace, double procentOfSegmentUptoAccident, int leftNodeIndex)
        {
            var leftNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex]);
            var rightNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex + 1]);

            var p1 = GisLabCalculator.GetPointAsPartOfSegmentOnPlaneEarth(leftNode.Position, rightNode.Position,
                procentOfSegmentUptoAccident);
            var p2 = GisLabCalculator.GetPointAsPartOfSegment(leftNode.Position, rightNode.Position,
                procentOfSegmentUptoAccident);
            return GisLabCalculator.InTheMiddle(p1, p2);
        }

        private void GetCableReserves(AccidentOnTraceV2 accident, Guid traceId, out double leftReserveM, out double rightReserveM)
        {
            var equipmentsWithoutPointsAndRtu = _model.GetTraceEquipmentsExcludingAdjustmentPoints(traceId).ToList();
            leftReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.Left!.LandmarkIndex, true);
            rightReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.Right!.LandmarkIndex, false);
        }

        private double GetCableReserve(List<Equipment> equipmentsWithoutPointsAndRtu, int landmarkIndex, bool isLeftLandmark)
        {
            if (landmarkIndex == 0)
                return 0; // RTU cannot contain cable reserve
            var equipment = equipmentsWithoutPointsAndRtu[landmarkIndex - 1];
            if (equipment.Type == EquipmentType.CableReserve) return (double)equipment.CableReserveLeft / 2;
            return isLeftLandmark ? equipment.CableReserveRight : equipment.CableReserveLeft;
        }

        private List<double>? GetGpsDistancesOfSegmentsBetweenLandmarks(AccidentOnTraceV2 accident, Trace trace, out Node? leftNode, out Node? rightNode)
        {
            var withoutPoints = _model.GetTraceNodesExcludingAdjustmentPoints(trace.TraceId).ToList();
            leftNode = _model.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.Left!.LandmarkIndex]);
            rightNode = _model.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.Right!.LandmarkIndex]);

            if (leftNode == null)
            {
                _logger.LogError(Logs.Client,$@"Node {withoutPoints[accident.Left?.LandmarkIndex ?? -1].First6()} not found");
                return null;
            }
            if (rightNode == null)
            {
                _logger.LogError(Logs.Client,$@"Node {withoutPoints[accident.Right?.LandmarkIndex ?? -1].First6()} not found");
                return null;
            }

            var indexOfLeft = trace.NodeIds.IndexOf(leftNode.NodeId);
            var indexOfRight = trace.NodeIds.IndexOf(rightNode.NodeId);
            var result = new List<double>();
            var fromNode = leftNode;
            for (int i = indexOfLeft + 1; i < indexOfRight; i++)
            {
                var toNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[i]);
                result.Add(GisLabCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, toNode.Position));
                fromNode = toNode;
            }
            result.Add(GisLabCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, rightNode.Position));
            return result;
        }
    }
}