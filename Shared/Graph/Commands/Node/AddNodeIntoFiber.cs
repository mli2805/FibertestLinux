using Fibertest.Dto;

namespace Fibertest.Graph
{
    /// <summary>
    /// Attention! Mind the difference with Fibertest 1.5
    /// This command for add node (well) only!
    /// Equipment should be added by separate command!
    /// </summary>
    public class AddNodeIntoFiber
    {
        public Guid Id;
        public PointLatLng Position;

        public Guid EquipmentId;
        public EquipmentType InjectionType;

        public Guid FiberId;
        public Guid NewFiberId1;
        public Guid NewFiberId2;

    }
}
