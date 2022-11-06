namespace Fibertest.Dto
{
    [Serializable]
    public class BranchOfAcceptableMeasParams
    {
        public Dictionary<string, LeafOfAcceptableMeasParams> Distances = new Dictionary<string, LeafOfAcceptableMeasParams>();
        public double BackscatteredCoefficient;
        public double RefractiveIndex;
    }
}