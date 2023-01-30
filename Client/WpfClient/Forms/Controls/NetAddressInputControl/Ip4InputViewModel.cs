using System.Linq;

namespace Fibertest.WpfClient
{
    public class Ip4InputViewModel
    {
        public string[] Parts { get; set; }

        public Ip4InputViewModel(string ip4Address)
        {
             // because of wrong IP strings like "" or "192.168.54" 
            Parts = ip4Address.Split('.');
            while (Parts.Length < 4)
            {
                ip4Address += @".";
                Parts = ip4Address.Split('.');
            }
        }

        public string GetString()
        {
            return $@"{Parts[0]}.{Parts[1]}.{Parts[2]}.{Parts[3]}";
        }

        public bool IsValidIpAddress()
        {
            return Parts.All(IsValidPartOfIpAddress);
        }

        private bool IsValidPartOfIpAddress(string part)
        {
            if (string.IsNullOrEmpty(part)) return false;
            if (!int.TryParse(part, out int res)) return false;
            if (res < 0 || res > 255) return false;
            return true;
        }
    }
}
