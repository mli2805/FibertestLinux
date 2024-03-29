﻿
namespace GMap.NET.WindowsPresentation
{
    using System.ComponentModel;
    using System.Windows;
    using NET;
    using System.Windows.Media;
    using System;

    /// <summary>
    /// GMap.NET marker
    /// </summary>
    public class GMapMarker : INotifyPropertyChanged
    {
        public Guid Id { get; set; }

        public Brush Color { get; set; }

        public double StrokeThickness { get; set; }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs name)
        {
            PropertyChanged?.Invoke(this, name);
        }

        UIElement shape;
        static readonly PropertyChangedEventArgs Shape_PropertyChangedEventArgs = new PropertyChangedEventArgs("Shape");

        /// <summary>
        /// marker visual
        /// </summary>
        public UIElement Shape
        {
            get { return shape; }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    OnPropertyChanged(Shape_PropertyChangedEventArgs);
                    UpdateLocalPosition();
                }
            }
        }

        

        private PointLatLng position;
        /// <summary>
        /// coordinate of marker
        /// </summary>
        public PointLatLng Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    UpdateLocalPosition();
                }
            }
        }

        public bool IsHighlighting { get; set; }

        GMapControl map;
        /// <summary>
        /// the map of this marker
        /// </summary>
        public GMapControl Map
        {
            get
            {
                if (Shape != null && map == null)
                {
                    DependencyObject visual = Shape;
                    while (visual != null && !(visual is GMapControl))
                    {
                        visual = VisualTreeHelper.GetParent(visual);
                    }

                    map = visual as GMapControl;
                }

                return map;
            }
            internal set { map = value; }
        }

        /// <summary>
        /// custom object
        /// </summary>
        public object Tag;

        Point offset;
        /// <summary>
        /// offset of marker
        /// </summary>
        public Point Offset
        {
            get { return offset; }
            set
            {
                if (offset != value)
                {
                    offset = value;
                    UpdateLocalPosition();
                }
            }
        }

        static readonly PropertyChangedEventArgs LocalPositionX_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionX");

        int localPositionX;
        /// <summary>
        /// local X position of marker
        /// </summary>
        public int LocalPositionX
        {
            get { return localPositionX; }
            internal set
            {
                if (localPositionX != value)
                {
                    localPositionX = value;
                    OnPropertyChanged(LocalPositionX_PropertyChangedEventArgs);
                }
            }
        }

        static readonly PropertyChangedEventArgs LocalPositionY_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionY");

        int localPositionY;
        /// <summary>
        /// local Y position of marker
        /// </summary>
        public int LocalPositionY
        {
            get { return localPositionY; }
            internal set
            {
                if (localPositionY != value)
                {
                    localPositionY = value;
                    OnPropertyChanged(LocalPositionY_PropertyChangedEventArgs);
                }
            }
        }

        static readonly PropertyChangedEventArgs ZIndex_PropertyChangedEventArgs = new PropertyChangedEventArgs("ZIndex");

        int zIndex;

        /// <summary>
        /// the index of Z, render order
        /// </summary>
        public int ZIndex
        {
            get { return zIndex; }
            set
            {
                if (zIndex != value)
                {
                    zIndex = value;
                    OnPropertyChanged(ZIndex_PropertyChangedEventArgs);
                }
            }
        }

        public GMapMarker(Guid id, PointLatLng pos, bool isHighlighting)
        {
            Id = id;
            Position = pos;
            IsHighlighting = isHighlighting;
        }

        internal GMapMarker()
        {
        }

        /// <summary>
        /// calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
        /// </summary>
        public virtual void Clear()
        {
            var s = (Shape as IDisposable);
            if (s != null)
            {
                s.Dispose();
                s = null;
            }
            Shape = null;
        }

        /// <summary>
        /// updates marker position, internal access usually
        /// </summary>
        void UpdateLocalPosition()
        {
            if (Map != null)
            {
                GPoint p = Map.FromLatLngToLocal(Position);
                p.Offset(-(long)Map.MapTranslateTransform.X, -(long)Map.MapTranslateTransform.Y);

                LocalPositionX = (int)(p.X + (long)(Offset.X));
                LocalPositionY = (int)(p.Y + (long)(Offset.Y));
            }
        }

        /// <summary>
        /// forces to update local marker  position
        /// dot not call it if you don't really need to ;}
        /// </summary>
        /// <param name="m"></param>
        internal void ForceUpdateLocalPosition(GMapControl m)
        {
            if (m != null)
            {
                map = m;
            }
            UpdateLocalPosition();
        }
    }
}
