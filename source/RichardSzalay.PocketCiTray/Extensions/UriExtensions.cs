using System;
using System.Linq;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Extensions
{
    public static class UriExtensions
    {
        public static IDictionary<string, string> GetQueryValues(this Uri uri)
        {
            // Query is inaccessible from relative uris
            var absoluteUri = new Uri(new Uri("http://tempuri.org", UriKind.Absolute), uri);

            return absoluteUri.Query.TrimStart('?').Split('&')
                .Select(kvp => kvp.Split('='))
                .ToDictionary(kvp => Uri.UnescapeDataString(kvp[0]), kvp => Uri.UnescapeDataString(kvp[1]));
        }
    }
}
