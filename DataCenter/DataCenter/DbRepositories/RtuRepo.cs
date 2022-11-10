namespace Fibertest.DataCenter
{
    public class RtuRepo
    {
        // Read it from DB
        public RtuStation? Get(Guid rtuId)
        {
            if (rtuId == Guid.Empty) return null;
            return new RtuStation()
            {
                RtuGuid = rtuId,
                MainAddress = "192.168.96.56",
                MainAddressPort = 11942,
                IsMainAddressOkDuePreviousCheck = true,
            };
        }

    }
}
