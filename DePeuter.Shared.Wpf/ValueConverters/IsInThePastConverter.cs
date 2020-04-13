using System;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class IsInThePastConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            return ((DateTime) value).Date < DateTime.Now.Date;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
