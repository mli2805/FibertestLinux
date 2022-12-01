namespace Fibertest.Dto
{
    public class RtuWithChannelChanges
    {
        public Guid RtuId;
        public RtuPartState MainChannel = RtuPartState.NotSetYet;
        public RtuPartState ReserveChannel = RtuPartState.NotSetYet;

        public string Report()
        {
            var mainChannel = MainChannel == RtuPartState.Broken
                    ? "Main channel is Broken"
                    : "Main channel Recovered";

            var reserveChannel = ReserveChannel == RtuPartState.NotSetYet
                ? ""
                : ReserveChannel == RtuPartState.Broken
                    ? "Reserve channel is Broken"
                    : "Reserve channel Recovered";

            return $"RTU {RtuId.First6()} " + mainChannel + reserveChannel;
        }
    }
}