using System;
using System.Globalization;
using System.Windows.Data;

namespace CSharpDemo.Converters
{
    public class FilterConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter,
            CultureInfo culture)
        {
            var p = System.Convert.ToInt32(parameter);

            if (value.Length == 1)
            {
                var d0 = (double)value[0];
                return ((int)d0).ToString();
            }
            else if (value.Length == 4)
            {
                var d0 = (double)value[0];
                var d1 = (double)value[1];
                var d2 = (double)value[2];
                var d3 = (double)value[3];

                var d4 = d3 * (d0 - d1) / (d2 - d1);
                return d4;
            }
            else if (value.Length == 5)
            {
                var d0 = (double)value[0];
                var d1 = (double)value[1];
                var d2 = (double)value[2];
                var d3 = (double)value[3];
                var d4 = (double)value[4];

                var d5 = d4 * (d0 - d1) / (d3 - d2);
                return d5;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}