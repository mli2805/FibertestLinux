using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    [Localizable(false)]
    public static class OtdrParamTemplatesProvider
    {
        private static readonly List<string> Titles = new List<string>()
        {
            Resources.SID_For_traces_0_05___20_km_long,
            Resources.SID__1__For_traces_0_05___0_5_km_long,
            Resources.SID__2__For_traces_0_5___5_km_long,
            Resources.SID__3__For_traces_5___10_km_long,
            Resources.SID__4__For_traces_10___20_km_long,
        };

        public static List<OtdrParametersTemplate> Get(Rtu rtu)
        {
            var is4100 = rtu.Omid == "RXT-4100+/1650 50dB";

            var result = new List<OtdrParametersTemplate>();
            result.Add(new OtdrParametersTemplate()
            {
                Id = 0,
                Title = Titles[0],
            });

            // временно для экспериментов Хазанова
            var ddd = rtu.AcceptableMeasParams.Units.Values.First().Distances.Last().Value.PulseDurations;
            var tps = TempGetTps(string.Join(", ", ddd));
            // ----------------------

            for (int i = 0; i < 4; i++)
            {
                result.Add(new OtdrParametersTemplate()
                {
                    Id = i + 1,
                    Title = Titles[i+1],
                    Lmax = is4100 ? AutoBaseParams.Rxt4100Lmax[i] : AutoBaseParams.Lmax[i],
                    Dl = is4100 ? AutoBaseParams.Rxt4100Dl[i] : AutoBaseParams.Dl[i],
                    // Tp = is4100 ? AutoBaseParams.Rxt4100Tp[i] : AutoBaseParams.Tp[i], // временно для экспериментов Хазанова
                    Tp = tps[i].Trim(),
                    Time = AutoBaseParams.Time[i],
                });
            }
           
            return result;
        }

        // временно для экспериментов Хазанова
        private static List<string> TempGetTps(string acceptableTps)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filename = basePath + "..\\ini\\temp_tps.txt";
            if (!File.Exists(filename))
                File.WriteAllLines(filename, new List<string>(){"Допустимые значения: "+acceptableTps, "25", "100", "100", "300"});
            var content = File.ReadAllLines(filename);
            return content.Skip(1).ToList(); // first line is comment
        }
    }
}