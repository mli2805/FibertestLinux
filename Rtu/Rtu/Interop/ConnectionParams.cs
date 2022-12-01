// ReSharper disable InconsistentNaming

namespace Fibertest.Rtu
{
    public struct ConnectionParams
    {
        public float reflectance; // R -dB
        public float splice; // dB (better say "loss")
        public float snr_almax;
    }
}
