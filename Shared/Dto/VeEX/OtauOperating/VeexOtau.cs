// ReSharper disable InconsistentNaming

namespace Fibertest.Dto
{
    [Serializable]
    public class VeexOtau
    {
        public bool connected;
        public string? id;
        public VeexOtauConnectionParameters? connectionParameters;
        public int inputPortCount;
        public bool isFwdm;
        public string? model;
        public int portCount;
        public string? protocol;
        public string? serialNumber;
    }
}
