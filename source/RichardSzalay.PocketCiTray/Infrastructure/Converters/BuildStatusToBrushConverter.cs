using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class BuildStatusToBrushConverter : IValueConverter
    {
        private readonly IDictionary<object, object> resources;

        public BuildStatusToBrushConverter()
            : this(Application.Current.Resources)
        {
            
        }

        public BuildStatusToBrushConverter(IDictionary<object, object> resources)
        {
            this.resources = resources;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (BuildResult) value;

            string resourceKey = String.Format("BuildResult{0:G}Brush", result);

            return resources[resourceKey];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
