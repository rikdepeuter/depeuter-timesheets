using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DePeuter.Shared.Wpf.ValueConverters
{
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                                Type targetType,
                                object parameter,
                                System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value is bool)
                {
                    if ((bool) value == false)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiAnyBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                                Type targetType,
                                object parameter,
                                System.Globalization.CultureInfo culture)
        {
            foreach(object value in values)
            {
                if(value is bool)
                {
                    if((bool)value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
