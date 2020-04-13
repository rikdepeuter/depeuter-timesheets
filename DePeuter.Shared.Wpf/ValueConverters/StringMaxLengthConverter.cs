using System;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class StringMaxLengthConverter : IValueConverter
    {
        private readonly object _parameter;

        public StringMaxLengthConverter()
        {
        }

        public StringMaxLengthConverter(object parameter)
        {
            _parameter = parameter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) return null;

            parameter = _parameter ?? parameter;

            if(parameter == null) throw new ArgumentException("Missing ConverterParameter");

            var maxLength = int.Parse(parameter.ToString());
            var svalue = value.ToString();

            if (svalue.Length <= maxLength) return svalue;
            return svalue.Substring(0, maxLength - 3) + "...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
