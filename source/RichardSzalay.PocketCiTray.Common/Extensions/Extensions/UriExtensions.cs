using System;
using System.Collections.Generic;
using System.Linq;

namespace RichardSzalay.PocketCiTray.Extensions.Extensions
{
    public static class UriExtensions
    {
        public static Uri MakeAbsolute(this Uri uri)
        {
            return (uri.IsAbsoluteUri)
                ? uri
                : new Uri(new Uri("http://tempuri.org", UriKind.Absolute), uri);
        }

        public static IDictionary<string, string> GetQueryValues(this Uri uri)
        {
            return uri.MakeAbsolute().Query.TrimStart('?').Split('&')
                .Select(kvp => kvp.Split('='))
                .ToDictionary(kvp => Uri.UnescapeDataString(kvp[0]), kvp => Uri.UnescapeDataString(kvp[1]));
        }

        public static bool IsSamePage(this Uri uri, Uri other)
        {
            return uri.MakeAbsolute().AbsolutePath ==
                other.MakeAbsolute().AbsolutePath;
        }

        public static Uri AppendQuery(this Uri uri, string query)
        {
            var builder = new UriBuilder(uri);

            builder.Query = (builder.Query + "&" + query).TrimStart('&');

            return builder.Uri;
        }
    }
}
