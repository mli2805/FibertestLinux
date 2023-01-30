using System;
using System.Globalization;
using System.Windows.Data;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class FrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var instance = (Frequency)value;
            return ConvertToString(instance, (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        private static string ConvertToString(Frequency instance, string param)
        {
            switch (instance)
            {
                case Frequency.DoNot: return param == @"0" ? Resources.SID_Do_not_measure : Resources.SID_Do_not_save;
                case Frequency.EveryHour: return Resources.SID_Every_hour;
                case Frequency.Every6Hours: return Resources.SID_Every_6_hours;
                case Frequency.Every12Hours: return Resources.SID_Every_12_hours;
                case Frequency.EveryDay: return Resources.SID_Every_day;
                case Frequency.Every2Days: return Resources.SID_Every_2_days;
                case Frequency.Every7Days: return Resources.SID_Every_7_days;
                case Frequency.Every30Days: return Resources.SID_Every_30_days;
                default: return Resources.SID_Wrong_param;
            }
        }
    }

    
}