using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class TraceModelBuilder
    {
        private readonly GraphGpsCalculator _graphGpsCalculator;

        public TraceModelBuilder(GraphGpsCalculator graphGpsCalculator)
        {
            _graphGpsCalculator = graphGpsCalculator;
        }

        public TraceModelForBaseRef GetTraceModelWithoutAdjustmentPoints(TraceModelForBaseRef traceModel)
        {
            var fullModel = GetTraceModel(traceModel);
            return ExcludeAdjustmentPoints(fullModel);
        }

        
        private TraceModelForBaseRef GetTraceModel(TraceModelForBaseRef model)
        {
            model.DistancesMm = new int[model.FiberArray!.Length];
            for (int i = 0; i < model.FiberArray.Length; i++)
            {
                var fiber = model.FiberArray[i];
                if (!fiber.UserInputedLength.Equals(0))
                    model.DistancesMm[i] = (int)fiber.UserInputedLength * 100;
                else
                    model.DistancesMm[i] = _graphGpsCalculator.CalculateDistanceBetweenNodesMm(
                        model.NodeArray[i], model.EquipArray[i], model.NodeArray[i + 1], model.EquipArray[i + 1]);
            }

            return model;
        }

        private TraceModelForBaseRef ExcludeAdjustmentPoints(TraceModelForBaseRef originalModel)
        {
            var nodes = new List<Node>(){originalModel.NodeArray[0]};
            var equipments = new List<Equipment>(){originalModel.EquipArray[0]};
            var distances = new List<int>();

            var distance = 0;
            for (int i = 1; i < originalModel.EquipArray.Length; i++)
            {
                distance += originalModel.DistancesMm![i - 1];

                if (originalModel.EquipArray[i].Type != EquipmentType.AdjustmentPoint)
                {
                    nodes.Add(originalModel.NodeArray[i]);
                    equipments.Add(originalModel.EquipArray[i]);
                    distances.Add(distance);
                    distance = 0;
                }
            }

            return new TraceModelForBaseRef(nodes.ToArray(),equipments.ToArray(), distances.ToArray());
        }

    }
}