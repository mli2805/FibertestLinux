using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class MonitoringCharonModel : PropertyChangedBase
    {
        public string Serial { get; set; }
        public string OtauId { get; set; }
        public bool IsMainCharon { get; set; }
        public int MainCharonPort { get; set; }
        public string Title { get; set; }


        private bool _groupenCheck;
        public bool GroupenCheck
        {
            get => _groupenCheck;
            set
            {
                _groupenCheck = value;
                foreach (var port in Ports.Where(p => p.IsReadyForMonitoring))
                {
                    port.IsIncluded = _groupenCheck;
                }
            }
        }

        public List<MonitoringPortModel> Ports { get; set; } = new List<MonitoringPortModel>();
        public bool IsEditEnabled { get; set; }


        public MonitoringCharonModel(string serial)
        {
            Serial = serial;
            Title = Serial;
        }

        public void SubscribeOnPortsChanges()
        {
            foreach (var port in Ports)
            {
                port.PropertyChanged += Port_PropertyChanged;
            }
        }

        private void Port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsIncluded")
                NotifyOfPropertyChange(nameof(CycleTime));
        }

        public int CycleTime =>
            Ports.Where(p => p.IsIncluded).Sum(p => (int)p.FastBaseSpan.TotalSeconds + 2); 
                // 2 sec for toggle port

    }
}