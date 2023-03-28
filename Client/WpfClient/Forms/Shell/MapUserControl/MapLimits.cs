using Caliburn.Micro;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class MapLimits : PropertyChangedBase
    {
        private readonly double _left;
        private readonly double _right;
        private readonly double _top;
        private readonly double _bottom;

        public MapLimits() { }

        public MapLimits(PointLatLng p1, PointLatLng p2)
        {
            if (p1.Lat > p2.Lat)
            {
                _top = p1.Lat;
                _bottom = p2.Lat;
            }
            else
            {
                _top = p2.Lat;
                _bottom = p1.Lat;
            }

            if (p1.Lng > p2.Lng)
            {
                _left = p2.Lng;
                _right = p1.Lng;
            }
            else
            {
                _left = p1.Lng;
                _right = p2.Lng;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="shift">shift is part of actual limits</param>
        /// <returns>whether point is in limits augmented by shift*limits at every side</returns>
        public bool IsInPlus(PointLatLng point, double shift)
        {
            var vertPad = (_top - _bottom) * shift;
            var horPad = (_right - _left) * shift;
            return !(point.Lat > _top + vertPad) && !(point.Lat < _bottom - vertPad)
                       && !(point.Lng < _left - horPad) && !(point.Lng > _right + horPad);
        }
    }
}
