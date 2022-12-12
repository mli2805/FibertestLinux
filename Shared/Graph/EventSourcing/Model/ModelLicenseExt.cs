namespace Fibertest.Graph
{
    public static class ModelLicenseExt
    {
        public static int GetRtuLicenseCount(this Model model)
        {
            return model.Licenses.Where(l=>l.RtuCount.ValidUntil >= DateTime.Today).Sum(l=>l.RtuCount.Value);
        } 
        
        public static int GetClientStationLicenseCount(this Model model)
        {
            return model.Licenses.Where(l=>l.ClientStationCount.ValidUntil >= DateTime.Today).Sum(l=>l.ClientStationCount.Value);
        } 
        
        public static int GetWebClientLicenseCount(this Model model)
        {
            return model.Licenses.Where(l=>l.WebClientCount.ValidUntil >= DateTime.Today).Sum(l=>l.WebClientCount.Value);
        } 
        
        public static int GetSuperClientStationLicenseCount(this Model model)
        {
            return model.Licenses.Where(l=>l.SuperClientStationCount.ValidUntil >= DateTime.Today).Sum(l=>l.SuperClientStationCount.Value);
        }

        public static bool IsMachineKeyRequired(this Model model)
        {
            return model.Licenses.Last().IsMachineKeyRequired;
        }
    }
}
