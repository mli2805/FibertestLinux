namespace Fibertest.Dto
{
    public class ServerForClient
    {
        public string Title { get; set; } = string.Empty;
        public DoubleAddress ServerAddress { get; set; } = new DoubleAddress();
        public bool IsLastSelected { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}