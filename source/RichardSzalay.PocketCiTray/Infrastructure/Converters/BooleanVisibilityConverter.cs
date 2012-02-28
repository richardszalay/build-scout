using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class BooleanVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBooleanVisibility(value, parameter)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private bool GetBooleanVisibility(object value, object parameter)
        {
            bool rawValue = (bool)value;

            return ((parameter as String) == "!")
                ? !rawValue
                : rawValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
