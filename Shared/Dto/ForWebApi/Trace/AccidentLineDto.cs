namespace Fibertest.Dto
{
    public class AccidentLineDto
    {
        public string? Caption;
     
        public int Number;
        public FiberState AccidentSeriousness;
        public string? AccidentTypeLetter;
        public AccidentPlace AccidentPlace;

        public string? TopLeft;
        public string? TopCenter;
        public string? TopRight;
        public string? Bottom0;
        public string? Bottom1;
        public string? Bottom2;
        public string? Bottom3;
        public string? Bottom4;
        public string? PngPath; 

        public GeoPoint? Position;
    }
}