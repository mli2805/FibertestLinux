using System;
using System.Collections.Generic;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class MeasurementEventArgs : EventArgs
    {
        public readonly ReturnCode Code;
        public readonly Trace? Trace;
        public readonly List<string>? AdditionalErrorLines;
        public readonly byte[]? SorBytes;

        // success
        public MeasurementEventArgs(ReturnCode code, Trace? trace, byte[]? sorBytes)
        {
            Code = code;
            Trace = trace;
            AdditionalErrorLines = null;
            SorBytes = sorBytes;
        }

        public MeasurementEventArgs(ReturnCode code, Trace trace, string errorMessage = "")
        {
            Code = code;
            Trace = trace;
            AdditionalErrorLines = new List<string>() { errorMessage };
            SorBytes = null;
        }

        public MeasurementEventArgs(ReturnCode code, Trace? trace, List<string> additionalErrorLines)
        {
            Code = code;
            Trace = trace;
            AdditionalErrorLines = additionalErrorLines;
            SorBytes = null;
        }
    }

}