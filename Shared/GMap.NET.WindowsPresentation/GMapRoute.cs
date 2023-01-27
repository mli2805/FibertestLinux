using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows.Shapes;

    public interface IShapable
    {
        void RegenerateShape(GMapControl map);
    }

    public class GMapRoute : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();
        public Guid LeftId { get; set; }
        public Guid RightId { get; set; }

        private int _askContextMenu;
        public int AskContextMenu
        {
            get { return _askContextMenu; }
            set
            {
                _askContextMenu = value;
                OnPropertyChanged("AskContextMenu");
            }
        }

        public ContextMenu ContextMenu { get; set; }

        public GMapRoute(Guid id, Guid leftId, Guid rightId, Brush color, double thickness, IEnumerable<PointLatLng> points, GMapControl map)
        {
            Id = id;
            LeftId = leftId;
            RightId = rightId;
            Color = color;
            StrokeThickness = thickness;
            Points.AddRange(points);
            RegenerateShape(map);
        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        /// <summary>
        /// regenerates shape of route
        /// </summary>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map == null) return;

            Map = map;

            if (Points.Count > 1)
            {
                Position = Points[0];

//                var localPath = new List<Point>(Points.Count) { new Point(0, 0) };
                var offset = Map.FromLatLngToLocal(Points[0]);
                GPoint p = Map.FromLatLngToLocal(Points[1]);
                var ppp = new Point(p.X - offset.X, p.Y - offset.Y);

//                localPath.Add(ppp);

//                File.AppendAllText(@"c:\temp\gmaproute.txt",
//                   $"{DateTime.Now}    0 ; 0 ; {ppp.X} ; {ppp.Y} ;" + Environment.NewLine);

                Shape = new Line() { X1 = 0, Y1 = 0, X2 = ppp.X, Y2 = ppp.Y, Stroke = Color, StrokeThickness = StrokeThickness };
            }
            else
                Shape = null;
        }

    }
}
