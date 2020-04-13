using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class StringReplaceConverter : IValueConverter
    {
        private readonly object _parameter;

        public StringReplaceConverter()
        {
        }

        public StringReplaceConverter(object parameter)
        {
            _parameter = parameter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) return null;

            parameter = _parameter ?? parameter;

            if(parameter == null) throw new ArgumentException("Missing ConverterParameter");

            var info = parameter as StringReplaceConverterParameter;
            if(info == null) throw new ArgumentException("Invalid ConverterParameter type");

            var svalue = value.ToString();

            if(info.Find != null)
            {
                foreach(var find in info.Find)
                {
                    svalue = svalue.Replace(find ?? string.Empty, info.Replace ?? string.Empty);
                }
            }

            return svalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringReplaceConverterParameter
    {
        public string[] Find { get; private set; }
        public string Replace { get; private set; }

        public StringReplaceConverterParameter(string find, string replace)
        {
            Find = new[] { find };
            Replace = replace;
        }

        public StringReplaceConverterParameter(string[] find, string replace)
        {
            Find = find;
            Replace = replace;
        }
    }
}
