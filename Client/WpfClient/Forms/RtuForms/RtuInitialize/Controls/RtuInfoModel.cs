using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class RtuInfoModel : PropertyChangedBase
    {
        private string _maker;
        public string Maker
        {
            get => _maker;
            set
            {
                if (value == _maker) return;
                _maker = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _mfid;
        public string? Mfid
        {
            get => _mfid;
            set
            {
                if (value == _mfid) return;
                _mfid = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _mfsn;
        public string? Mfsn
        {
            get => _mfsn;
            set
            {
                if (value == _mfsn) return;
                _mfsn = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _omid;
        public string? Omid
        {
            get => _omid;
            set
            {
                if (value == _omid) return;
                _omid = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _omsn;
        public string? Omsn
        {
            get => _omsn;
            set
            {
                if (value == _omsn) return;
                _omsn = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mainOtauModel;
        public string MainOtauModel
        {
            get => _mainOtauModel;
            set
            {
                if (value == _mainOtauModel) return;
                _mainOtauModel = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mainOtauSerial;
        public string MainOtauSerial
        {
            get => _mainOtauSerial;
            set
            {
                if (value == _mainOtauSerial) return;
                _mainOtauSerial = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _version;
        public string? Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                NotifyOfPropertyChange();
            }
        }


        private string? _version2;
        public string? Version2
        {
            get => _version2;
            set
            {
                if (value == _version2) return;
                _version2 = value;
                NotifyOfPropertyChange();
            }
        }


        private Visibility _visibility;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }


        public void FromRtu(Rtu rtu)
        {
            Maker = rtu.RtuMaker.ToString();
            Mfid = rtu.Mfid;
            Mfsn = rtu.Mfsn;
            Omid = rtu.Omid;
            Omsn = rtu.Omsn;
            Version = rtu.Version;
            Version2 = rtu.Version2;

            MainOtauModel = rtu.RtuMaker == RtuMaker.VeEX ? rtu.MainVeexOtau.model ?? @"n/a" : "";
            MainOtauSerial = rtu.RtuMaker == RtuMaker.VeEX ? rtu.MainVeexOtau.serialNumber ?? @"n/a" : "";
        }

        public void FromDto(RtuInitializedDto dto)
        {
            Maker = dto.Maker.ToString();
            Mfid = dto.Mfid;
            Mfsn = dto.Mfsn;
            Omid = dto.Omid;
            Omsn = dto.Omsn;
            Version = dto.Version;
            Version2 = dto.Version2;

            MainOtauModel = dto.Maker == RtuMaker.VeEX ? dto.MainVeexOtau.model ?? @"n/a" : "";
            MainOtauSerial = dto.Maker == RtuMaker.VeEX ? dto.MainVeexOtau.serialNumber ?? @"n/a" : "";
        }
    }
}