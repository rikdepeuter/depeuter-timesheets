using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class MinutesDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var totalMinutes = value as double?;
            if (totalMinutes == null)
            {
                return null;
            }
            var isNegative = totalMinutes < 0;
            totalMinutes = Math.Abs(totalMinutes.Value);
            var hours = (int)(totalMinutes/60);
            var minutes = totalMinutes - hours*60;
            return string.Format("{0}{1}:{2:00}", isNegative ? "-" : "", hours, minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (strValue == null)
            {
                return null;
            }

            double minutes;
            var data = strValue.Split(':');
            if (data.Length == 1)
            {
                minutes = double.Parse(data[0]);
            }
            else
            {
                var hours = double.Parse(data[0]);
                minutes = double.Parse(data[1]) + hours * 60;
            }

            return minutes;
        }
    }
}
