using GMap.NET;

namespace Fibertest.Graph;

public static class PointLatLngExtensions
{
    public static string ToDetailedString(this PointLatLng pointLatLng, GpsInputMode mode)
    {
        string degreeSign = @"°";
        string minuteSign = @"′";
        string secondSign = @"″";
        if (mode == GpsInputMode.Degrees)
        {
            return $@"{pointLatLng.Lat:00.000000}{degreeSign}   {pointLatLng.Lng:00.000000}{degreeSign}";
        }
        if (mode == GpsInputMode.DegreesAndMinutes)
        {
            int dLat = (int)pointLatLng.Lat;
            double mLat = (pointLatLng.Lat - dLat) * 60;

            int dLng = (int)pointLatLng.Lng;
            double mLng = (pointLatLng.Lng - dLng) * 60;

            return $@"{dLat:00}{degreeSign} {mLat:00.0000}{minuteSign}    {dLng:00}{degreeSign} {mLng:00.0000}{minuteSign}";
        }
        if (mode == GpsInputMode.DegreesMinutesAndSeconds)
        {
            int dLat = (int)pointLatLng.Lat;
            double mLat = (pointLatLng.Lat - dLat) * 60;
            int miLat = (int)mLat;
            double sLat = (mLat - miLat) * 60;

            int dLng = (int)pointLatLng.Lng;
            double mLng = (pointLatLng.Lng - dLng) * 60;
            int miLng = (int)mLng;
            double sLng = (mLng - miLng) * 60;

            return $@"{dLat:00}{degreeSign} {miLat:00}{minuteSign} {sLat:00.00}{secondSign}     {dLng:00}{degreeSign} {miLng:00}{minuteSign} {sLng:00.00}{secondSign}";
        }
        return "";
    }

}