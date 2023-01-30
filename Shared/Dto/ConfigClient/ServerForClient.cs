namespace Fibertest.Dto
{
    public class ServerForClient
    {
        public string Title { get; set; }
        public DoubleAddress ServerAddress { get; set; }
        public bool IsLastSelected { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}