﻿using System;
using System.Data.Linq.Mapping;
using System.Net;

namespace RichardSzalay.PocketCiTray
{
    [Table]
    public class BuildServer
    {
        public int Id { get; set; }

        public NetworkCredential Credential { get; set; }

        public Uri Uri { get; set; }

        public string Name { get; set; }

        public string Provider { get; set; }

        public static BuildServer FromUri(String provider, Uri uri)
        {
            var builder = new UriBuilder(uri);

            NetworkCredential credential = (builder.UserName != "")
                ? new NetworkCredential(builder.UserName, builder.Password)
                : null;

            return new BuildServer
            {
                Name = builder.Host,
                Uri = builder.Uri,
                Credential = credential,
                Provider = provider
            };
        }
    }
}