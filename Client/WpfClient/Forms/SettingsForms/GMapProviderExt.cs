using GMap.NET;

namespace Fibertest.WpfClient
{
    public static class GMapProviderExt
    {
        public static GMapProvider Get(string providerName)
        {
            switch (providerName)
            {
                case "OpenStreetMap":
                    {
                        return GMapProviders.OpenStreetMap;
                    }
                case "GoogleMap":
                    {
                        return GMapProviders.GoogleMap;
                    }
                case "GoogleSatelliteMap":
                    {
                        return GMapProviders.GoogleSatelliteMap;
                    }
                case "GoogleHybridMap":
                    {
                        return GMapProviders.GoogleHybridMap;
                    }
                // case "YandexMap":
                //     {
                //         return GMapProviders.YandexMap;
                //     }
                default:
                    return GMapProviders.EmptyProvider;
            }
        }
    }
}