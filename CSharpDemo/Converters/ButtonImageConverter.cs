using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CSharpDemo.Converters
{
    public class ButtonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new BitmapImage(new Uri(@"..\..\Image\t_icon3.png", UriKind.Relative));
            }

            if ((int)value == 0)
            {
                return new BitmapImage(new Uri(@"..\..\Image\t_icon3.png", UriKind.Relative));
            }

            return new BitmapImage(new Uri(@"..\..\Image\t_icon6.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}