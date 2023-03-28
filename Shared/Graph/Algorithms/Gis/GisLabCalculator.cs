using GMap.NET;

namespace Fibertest.Graph
{
    public static class GisLabCalculator
    {
        const int EarthRadius = 6372795;

        public static double GetDistanceBetweenPointLatLng(PointLatLng p1, PointLatLng p2, out double azimuth)
        {
            var q1 = new CoorsInRad(p1);
            var q2 = new CoorsInRad(p2);
            return GetDistanceBetweenPoints(q1, q2, out azimuth);
        }

        public static double GetDistanceBetweenPointLatLng(PointLatLng p1, PointLatLng p2)
        {
            var q1 = new CoorsInRad(p1);
            var q2 = new CoorsInRad(p2);
            return GetDistanceBetweenPoints(q1, q2, out _);
        }

        public static PointLatLng GetPointAsPartOfSegment(PointLatLng p1, PointLatLng p2, double part)
        {
            var q1 = new CoorsInRad(p1);
            var q2 = new CoorsInRad(p2);

            // обратная геодезическая задача
            var decCoors = SphereToDecart(q2);
            decCoors = RotateAround(decCoors, q1.Lng, DecartAxis.Z);
            decCoors = RotateAround(decCoors, Math.PI / 2 - q1.Lat, DecartAxis.Y);
            CoorsInRad q = DecartToSphere(decCoors);
            double azimuth = Math.PI - q.Lng;
            var distanceOnSphere = Math.PI / 2 - q.Lat;

            // часть отрезка
            var distanceOnSphere2 = distanceOnSphere * part;

            // прямая геодезическая задача
            var sph = new CoorsInRad(Math.PI / 2 - distanceOnSphere2, Math.PI - azimuth);
            var decartCoors = SphereToDecart(sph);
            decartCoors = RotateAround(decartCoors, q1.Lat - Math.PI / 2, DecartAxis.Y);
            decartCoors = RotateAround(decartCoors, -q1.Lng, DecartAxis.Z);
            var coorsInRad = DecartToSphere(decartCoors);
            return new PointLatLng(coorsInRad.Lat / Math.PI * 180, coorsInRad.Lng / Math.PI * 180);
        }

        // q1, q2 - point coors in radians
        private static double GetDistanceBetweenPoints(CoorsInRad q1, CoorsInRad q2, out double azimuth)
        {
            var decCoors = SphereToDecart(q2);
            decCoors = RotateAround(decCoors, q1.Lng, DecartAxis.Z);
            decCoors = RotateAround(decCoors, Math.PI / 2 - q1.Lat, DecartAxis.Y);
            var q = DecartToSphere(decCoors);
            azimuth = Math.PI - q.Lng;
            var distanceOnSphere = Math.PI / 2 - q.Lat;
            var distance = distanceOnSphere * EarthRadius;
            return distance;
        }


        private static DecartCoors SphereToDecart(CoorsInRad sphere)
        {
            double p = Math.Cos(sphere.Lat);
            var result = new DecartCoors
            {
                Z = Math.Sin(sphere.Lat),
                Y = p * Math.Sin(sphere.Lng),
                X = p * Math.Cos(sphere.Lng)
            };

            return result;
        }

        private static CoorsInRad DecartToSphere(DecartCoors coors)
        {
            var hypot = Math.Sqrt(coors.X * coors.X + coors.Y * coors.Y);
            return new CoorsInRad
            {
                Lng = Math.Atan2(coors.Y, coors.X),
                Lat = Math.Atan2(coors.Z, hypot)
            };

            // нафиг не нужен
            // return Math.Sqrt(hypot * hypot + coors.Z * coors.Z);
        }


        private static DecartCoors RotateAround(DecartCoors coors, double alpha, DecartAxis axes)
        {
            switch (axes)
            {
                case DecartAxis.X:
                    return new DecartCoors()
                    {
                        X = coors.X,
                        Y = coors.Y * Math.Cos(alpha) + coors.Z * Math.Sin(alpha),
                        Z = -coors.Y * Math.Sin(alpha) + coors.Z * Math.Cos(alpha),
                    };
                case DecartAxis.Y:
                    return new DecartCoors()
                    {
                        X = -coors.Z * Math.Sin(alpha) + coors.X * Math.Cos(alpha),
                        Y = coors.Y,
                        Z = coors.Z * Math.Cos(alpha) + coors.X * Math.Sin(alpha),
                    };
                // case DecartAxis.Z:
                default:
                    return new DecartCoors()
                    {
                        X = coors.X * Math.Cos(alpha) + coors.Y * Math.Sin(alpha),
                        Y = -coors.X * Math.Sin(alpha) + coors.Y * Math.Cos(alpha),
                        Z = coors.Z,
                    };
            }
        }

        // in meters
        public static double GetDistanceBetweenPointLatLngOldMethod(PointLatLng p1, PointLatLng p2)
        {
            var q1 = new CoorsInRad(p1);
            var q2 = new CoorsInRad(p2);

            var lat1Cos = Math.Cos(q1.Lat);
            var lat2Cos = Math.Cos(q2.Lat);
            var lat1Sin = Math.Sin(q1.Lat);
            var lat2Sin = Math.Sin(q2.Lat);

            var sinDeltaLong = Math.Sin(q2.Lng - q1.Lng);
            var cosDeltaLong = Math.Cos(q2.Lng - q1.Lng);

            var y = Math.Sqrt(Math.Pow(lat2Cos * sinDeltaLong, 2) + 
                              Math.Pow(lat1Cos * lat2Sin - lat1Sin * lat2Cos * cosDeltaLong, 2));
            var x = lat1Sin * lat2Sin + lat1Cos * lat2Cos * cosDeltaLong;

            return Math.Atan2(y, x) * EarthRadius;
        }
        
        public static PointLatLng GetPointAsPartOfSegmentOnPlaneEarth(PointLatLng p1, PointLatLng p2, double part)
        {
            var lat = p1.Lat + (p2.Lat - p1.Lat) * part;
            var lng = p1.Lng + (p2.Lng - p1.Lng) * part;

            return new PointLatLng(lat, lng);
        }

        public static int GpsInSorFormat(double degrees)
        {
            int d = (int)degrees;
            double m = (degrees - d) * 60;
            int mi = (int)m;
            double s = (m - mi) * 60;
            int ss = (int)(s * 100);
            if (s * 100 - ss >= 0.5) ss++;
            return d * 1000000 + mi * 10000 + ss;
        }

        public static PointLatLng InTheMiddle(PointLatLng p1, PointLatLng p2)
        {
            return new PointLatLng((p1.Lat + p2.Lat) / 2, (p1.Lng + p2.Lng) / 2);
        }
    }
}