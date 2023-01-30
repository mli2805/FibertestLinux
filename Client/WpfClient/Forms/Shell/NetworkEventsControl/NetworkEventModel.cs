using System;
using System.Windows.Media;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

// ReSharper disable InconsistentNaming

namespace Fibertest.WpfClient
{
    public class NetworkEventModel
    {
        public int Ordinal { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

      //  private bool IsRtuAvailable => MainChannel == RtuPartState.Ok || ReserveChannel == RtuPartState.Ok;
        public bool IsRtuAvailable;
        public string RtuAvailabilityString => IsRtuAvailable ? Resources.SID_Available : Resources.SID_Not_available;
        public Brush RtuAvailabilityBrush => GetAvailabilityBrush();

        public RtuPartState MainChannel { get; set; }
        public ChannelEvent OnMainChannel { get; set; }

        public string MainChannelEventString => OnMainChannel == ChannelEvent.Nothing 
            ? MainChannel.ToLocalizedString() 
            : OnMainChannel.ToLocalizedString();
        public Brush MainChannelEventBrush => OnMainChannel.GetBrush(false);

        public RtuPartState ReserveChannel { get; set; }
        public ChannelEvent OnReserveChannel { get; set; }
        public string ReserveChannelEventString => OnReserveChannel == ChannelEvent.Nothing 
            ? ReserveChannel.ToLocalizedString() 
            : OnReserveChannel.ToLocalizedString();
        public Brush ReserveChannelEventBrush => OnReserveChannel.GetBrush(false);


        private Brush GetAvailabilityBrush()
        {
            if (MainChannel == RtuPartState.Ok && ReserveChannel != RtuPartState.Broken)
                return Brushes.Transparent;

            if (((int) MainChannel + (int) ReserveChannel) == 0)
                return Brushes.LightPink;

            return Brushes.Red;
        }
    }
}
