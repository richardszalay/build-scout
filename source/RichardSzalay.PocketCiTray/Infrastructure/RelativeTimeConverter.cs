using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class RelativeTimeConverter : IValueConverter
    {
        private readonly IClock clock;

        public RelativeTimeConverter()
            : this(new DateTimeOffsetClock())
        {
        }

        public RelativeTimeConverter(IClock clock)
        {
            this.clock = clock;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = ((DateTimeOffset) value).ToUniversalTime();
            var now = clock.UtcNow;
            var diff = now - date;

            if (now.Date == date.Date)
            {
                return date.ToString("t");
            }
            
            if (diff < TimeSpan.FromDays(7))
            {
                return date.ToString("ddd");
            }

            return date.ToString(Strings.ShortDatePattern);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
