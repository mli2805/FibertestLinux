using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class OtdrParametersTemplateModel : PropertyChangedBase, IDataErrorInfo
    {

        public List<string> Units { get; set; } = null!;

        private string _selectedUnit = null!;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (value == _selectedUnit) return;
                _selectedUnit = value;
                NotifyOfPropertyChange();
            }
        }

        private double _backScatteredCoefficient;
        public double BackScatteredCoefficient
        {
            get => _backScatteredCoefficient;
            set
            {
                if (value.Equals(_backScatteredCoefficient)) return;
                _backScatteredCoefficient = value;
                NotifyOfPropertyChange();
            }
        }

        private double _refractiveIndex;
        public double RefractiveIndex
        {
            get => _refractiveIndex;
            set
            {
                if (value.Equals(_refractiveIndex)) return;
                _refractiveIndex = value;
                NotifyOfPropertyChange();
            }
        }

        public List<OtdrParametersTemplate> OtdrParametersTemplates { get; set; } = new List<OtdrParametersTemplate>();
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        private OtdrParametersTemplate _selectedOtdrParametersTemplate = null!;
        public OtdrParametersTemplate SelectedOtdrParametersTemplate
        {
            get => _selectedOtdrParametersTemplate;
            set
            {
                if (Equals(value, _selectedOtdrParametersTemplate)) return;
                _selectedOtdrParametersTemplate = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(Rtu rtu)
        {
            OtdrParametersTemplates.Clear();
            var templates = OtdrParamTemplatesProvider.Get(rtu);
            Title = templates[0].Title;
            Description = templates[0].Description;
            foreach (var template in templates)
            {
                template.PropertyChanged += Template_PropertyChanged;
                OtdrParametersTemplates.Add(template);
            }

            var defaultTemplate = 0;
            OtdrParametersTemplates[defaultTemplate].IsChecked = true;
            SelectedOtdrParametersTemplate = OtdrParametersTemplates[defaultTemplate];
        }

        private void Template_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsChecked")
            {
                SelectedOtdrParametersTemplate = OtdrParametersTemplates.First(t => t.IsChecked);
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    // case "Model.BackScatteredCoefficient":
                    // if (Model.BackScatteredCoefficient < -100 || Model.BackScatteredCoefficient > -60)
                    // errorMessage = Resources.SID_Invalid_input;
                    // break;
                    case "RefractiveIndex":
                        if (RefractiveIndex < 1 || RefractiveIndex > 2)
                            errorMessage = Resources.SID_Invalid_input;
                        break;
                }

                return errorMessage;
            }
        }

        public string Error { get; set; } = string.Empty;
    }
}