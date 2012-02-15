using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class RelativeTimeConverter : IValueConverter
    {
        public RelativeTimeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = ((DateTimeOffset) value).ToUniversalTime();
            var now = DateTimeOffset.UtcNow;
            var diff = now - date;
            
            string resourceKey = GetResourceKey(date, now, diff);

            return String.Format(Strings.ResourceManager.GetString(resourceKey), diff, date);
        }

        private static string GetResourceKey(DateTimeOffset date, DateTimeOffset now, TimeSpan diff)
        {
            if (diff < TimeSpan.FromMinutes(1))
            {
                return "LessThanOneMinuteAgo";
            }
            if (diff < TimeSpan.FromMinutes(2))
            {
                return "OneMinuteAgo";
            }
            if (date.Date == now.Date)
            {
                return "AtTime";
            }
            if (date.Date == now.Date.AddDays(-1))
            {
                return "Yesterday";
            }
            if (diff < TimeSpan.FromDays(7))
            {
                return "ThisWeek";
            }

            return "OnDate";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
