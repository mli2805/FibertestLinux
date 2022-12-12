namespace Fibertest.Graph
{
    public static class OneRtuExportImport
    {
        public static Model CreateOneRtuModel(this Model readModel, Rtu rtu)
        {
            var oneRtuGraphModel = new Model();
            oneRtuGraphModel.Rtus.Add(rtu);

            foreach (var otau in readModel.Otaus.Where(o => o.RtuId == rtu.Id))
            {
                oneRtuGraphModel.Otaus.Add(otau);
            }

            foreach (var trace in readModel.Traces.Where(t => t.RtuId == rtu.Id))
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    if (oneRtuGraphModel.Nodes.All(n => n.NodeId != nodeId))
                        oneRtuGraphModel.Nodes.Add(readModel.Nodes.First(n => n.NodeId == nodeId));
                }

                for (int i = 1; i < trace.EquipmentIds.Count; i++)
                {
                    if (oneRtuGraphModel.Equipments.All(n => n.EquipmentId != trace.EquipmentIds[i]))
                        oneRtuGraphModel.Equipments.Add(readModel.Equipments.First(n => n.EquipmentId == trace.EquipmentIds[i]));
                }

                foreach (var fiberId in trace.FiberIds)
                {
                    if (oneRtuGraphModel.Fibers.All(n => n.FiberId != fiberId))
                        oneRtuGraphModel.Fibers.Add(readModel.Fibers.First(n => n.FiberId == fiberId));
                }

                oneRtuGraphModel.Traces.Add(trace);
            }


            return oneRtuGraphModel;
        }

    }
}
