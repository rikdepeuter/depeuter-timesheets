using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class AnyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as ICollection;
            if (collection == null) return false;

            return collection.Count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
