using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = Convert2.ToNullableDouble(value, System.Globalization.CultureInfo.InvariantCulture);

            var compare = Convert2.ToNullableDouble(parameter, System.Globalization.CultureInfo.InvariantCulture);

            if (number == null || parameter == null) return null;
            return number > compare;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
