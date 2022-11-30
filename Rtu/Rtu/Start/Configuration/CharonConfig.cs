namespace Fibertest.Rtu
{
    public class CharonConfig
    {
        public string? ComPortName { get; set; }
        public int ComPortSpeed { get; set; }

        private int _pauseAfterReset;
        public int PauseAfterReset
        {
            get { return _pauseAfterReset == 0 ? 5 : _pauseAfterReset; }
            set => _pauseAfterReset = value;
        }
    }
}
