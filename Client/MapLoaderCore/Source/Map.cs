using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation
{
    /// <summary>
    /// the custom map f GMapControl 
    /// </summary>
    public class Map : GMapControl
    {
        public long ElapsedMilliseconds;

        readonly Typeface _tf = new Typeface("GenericSansSerif");
        readonly FlowDirection _fd = new FlowDirection();

        /// <summary>
        /// any custom drawing here
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            FormattedText text = new FormattedText(string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider, 
                        CultureInfo.InvariantCulture, _fd, _tf, 20, Brushes.Blue);
            drawingContext.DrawText(text, new Point(text.Height, text.Height));
        }
    }
}