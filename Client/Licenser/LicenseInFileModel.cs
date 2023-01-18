using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Licenser
{
    public class LicenseInFileModel : PropertyChangedBase
    {
        public List<string> TermUnit { get; set; } = new List<string>() { "years", "months" };

        private Guid _licenseId;
        public Guid LicenseId
        {
            get => _licenseId;
            set
            {
                if (value.Equals(_licenseId)) return;
                _licenseId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
                NotifyOfPropertyChange(nameof(LicenseKeyColor));
            }
        }

        public string LicenseKey => Lk();

        private string Lk()
        {
            if (LicenseId == Guid.Empty)
                return "FT030-00000000-000000000-000000";

            var id = LicenseId.ToString().ToUpper().Substring(0, 8);
            var licType = IsIncremental ? "I" : IsMachineKeyRequired ? "BR" : "BF";
            var stations = $"{ClientStationCount:D2}{WebClientCount:D2}{SuperClientStationCount:D2}";
            return $"FT030-{id}-{licType}{RtuCount:D2}{stations}-{CreationDate:yyMMdd}";
        }

        public Brush LicenseKeyColor => LicenseId == Guid.Empty ? Brushes.Gray : Brushes.Black;

        private string _owner = "";
        public string Owner
        {
            get => _owner;
            set
            {
                if (value == _owner) return;
                _owner = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isIncremental;
        public bool IsIncremental
        {
            get => _isIncremental;
            set
            {
                if (value == _isIncremental) return;
                _isIncremental = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
                NotifyOfPropertyChange(nameof(IsWebClientsEnabled));
            }
        }

        private bool _isBasic;
        public bool IsBasic
        {
            get => _isBasic;
            set
            {
                if (value == _isBasic) return;
                _isBasic = value;
                if (_isMachineKeyRequired && _isBasic) 
                    WebClientCount = 0;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(WebClientCount));  NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsWebClientsEnabled));
            }
        }

        private int _rtuCount;
        public int RtuCount
        {
            get => _rtuCount;
            set
            {
                if (value == _rtuCount) return;
                _rtuCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _rtuCountTerm = 999;
        public int RtuCountTerm
        {
            get => _rtuCountTerm;
            set
            {
                if (value == _rtuCountTerm) return;
                _rtuCountTerm = value;
                NotifyOfPropertyChange();
            }
        }

        private string _rtuCountTermUnit = "";
        public string RtuCountTermUnit
        {
            get => _rtuCountTermUnit;
            set
            {
                if (value == _rtuCountTermUnit) return;
                _rtuCountTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        private int _clientStationCount;
        public int ClientStationCount
        {
            get => _clientStationCount;
            set
            {
                if (value == _clientStationCount) return;
                _clientStationCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _clientStationTerm = 999;
        public int ClientStationTerm
        {
            get => _clientStationTerm;
            set
            {
                if (value == _clientStationTerm) return;
                _clientStationTerm = value;
                NotifyOfPropertyChange();
            }
        }

        private string _clientStationTermUnit = "";
        public string ClientStationTermUnit
        {
            get => _clientStationTermUnit;
            set
            {
                if (value == _clientStationTermUnit) return;
                _clientStationTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        private int _webClientCount;
        public int WebClientCount
        {
            get => _webClientCount;
            set
            {
                if (value == _webClientCount) return;
                _webClientCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }


        private int _webClientTerm = 999;
        public int WebClientTerm
        {
            get => _webClientTerm;
            set
            {
                if (value == _webClientTerm) return;
                _webClientTerm = value;
                NotifyOfPropertyChange();
            }
        }

        private string _webClientTermUnit = "";

        public string WebClientTermUnit
        {
            get => _webClientTermUnit;
            set
            {
                if (value == _webClientTermUnit) return;
                _webClientTermUnit = value;
                NotifyOfPropertyChange();
            }
        }


        private int _superClientStationCount;
        public int SuperClientStationCount
        {
            get => _superClientStationCount;
            set
            {
                if (value == _superClientStationCount) return;
                _superClientStationCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _superClientTerm = 999;
        public int SuperClientTerm
        {
            get => _superClientTerm;
            set
            {
                if (value == _superClientTerm) return;
                _superClientTerm = value;
                NotifyOfPropertyChange();
            }
        }

        private string _superClientTermUnit = "";
        public string SuperClientTermUnit
        {
            get => _superClientTermUnit;
            set
            {
                if (value == _superClientTermUnit) return;
                _superClientTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isMachineKeyRequired;
        public bool IsMachineKeyRequired
        {
            get => _isMachineKeyRequired;
            set
            {
                if (value == _isMachineKeyRequired) return;
                _isMachineKeyRequired = value;
                if (_isMachineKeyRequired && _isBasic) 
                    WebClientCount = 0;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(WebClientCount));
                NotifyOfPropertyChange(nameof(LicenseKey));
                NotifyOfPropertyChange(nameof(IsWebClientsEnabled));
            }
        }

        private bool _isStandart;
        public bool IsStandart
        {
            get => _isStandart;
            set
            {
                if (value == _isStandart) return;
                _isStandart = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsWebClientsEnabled));
            }
        }

        public bool IsWebClientsEnabled => IsStandart || IsIncremental;

        public string? SecurityAdminPassword { get; set; }


        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                if (value.Equals(_creationDate)) return;
                _creationDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        public LicenseInFileModel()
        {
            LicenseId = Guid.Empty;
            IsBasic = true;
            IsStandart = true;
            RtuCountTermUnit = TermUnit.First();
            ClientStationTermUnit = TermUnit.First();
            WebClientTermUnit = TermUnit.First();
            SuperClientTermUnit = TermUnit.First();
        }

        public LicenseInFileModel(LicenseInFile licenseInFile)
        {
            LicenseId = licenseInFile.LicenseId;
            Owner = licenseInFile.Owner;
            IsIncremental = licenseInFile.IsIncremental;
            IsBasic = !licenseInFile.IsIncremental;

            RtuCount = licenseInFile.RtuCount.Value;
            RtuCountTerm = licenseInFile.RtuCount.Term;
            RtuCountTermUnit = licenseInFile.RtuCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            ClientStationCount = licenseInFile.ClientStationCount.Value;
            ClientStationTerm = licenseInFile.ClientStationCount.Term;
            ClientStationTermUnit = licenseInFile.ClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            WebClientCount = licenseInFile.WebClientCount.Value;
            WebClientTerm = licenseInFile.WebClientCount.Term;
            WebClientTermUnit = licenseInFile.WebClientCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            SuperClientStationCount = licenseInFile.SuperClientStationCount.Value;
            SuperClientTerm = licenseInFile.SuperClientStationCount.Term;
            SuperClientTermUnit = licenseInFile.SuperClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            IsMachineKeyRequired = licenseInFile.IsMachineKeyRequired;
            IsStandart = !licenseInFile.IsMachineKeyRequired;
            SecurityAdminPassword = (string?)Cryptography.Decode(licenseInFile.SecurityAdminPassword);
            CreationDate = licenseInFile.CreationDate;

        }

        public LicenseInFile ToLicenseInFile()
        {
            return new LicenseInFile()
            {
                LicenseId = LicenseId,
                Owner = Owner,
                IsIncremental = IsIncremental,
                RtuCount = new LicenseParameterInFile()
                {
                    Value = RtuCount,
                    Term = RtuCountTerm,
                    IsTermInYears = RtuCountTermUnit == TermUnit.First(),
                },
                ClientStationCount = new LicenseParameterInFile()
                {
                    Value = ClientStationCount,
                    Term = ClientStationTerm,
                    IsTermInYears = ClientStationTermUnit == TermUnit.First(),
                },
                WebClientCount = new LicenseParameterInFile()
                {
                    Value = WebClientCount,
                    Term = WebClientTerm,
                    IsTermInYears = WebClientTermUnit == TermUnit.First(),
                },
                SuperClientStationCount = new LicenseParameterInFile()
                {
                    Value = SuperClientStationCount,
                    Term = SuperClientTerm,
                    IsTermInYears = SuperClientTermUnit == TermUnit.First(),
                },
                IsMachineKeyRequired = IsMachineKeyRequired,
                SecurityAdminPassword = Cryptography.Encode(SecurityAdminPassword),
                CreationDate = CreationDate,
            };
        }


    }
}