using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;

            var collection = value as IList;
            if (collection == null) return null;

            return collection.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
