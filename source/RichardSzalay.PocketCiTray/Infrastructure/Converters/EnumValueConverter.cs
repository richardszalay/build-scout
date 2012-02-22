using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using System.Windows.Markup;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    [ContentProperty("Values")]
    public class EnumValueConverter : IValueConverter
    {
        private List<EnumValue> values = new List<EnumValue>();
        public List<EnumValue> Values { get { return values; } }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string stringValue = String.Format("{0:G}", value);

            foreach (EnumValue enumValue in Values)
            {
                if (enumValue.Key == (string)stringValue)
                {
                    return enumValue.Value;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ContentProperty("Value")]
    public class EnumValue
    {
        public string Key { get; set; }
        
        public object Value { get; set; }
    }
}
