using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class LocalizedBuildStatusConverter : IValueConverter
    {
        private readonly ResourceManager resources;

        public LocalizedBuildStatusConverter()
            : this(Strings.ResourceManager)
        {
            
        }

        public LocalizedBuildStatusConverter(ResourceManager resources)
        {
            this.resources = resources;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((BuildResult) value);
        }

        public string Convert(BuildResult value)
        {
            var result = (BuildResult)value;

            string resourceKey = String.Format("BuildResult_{0:G}", result);

            return resources.GetString(resourceKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
