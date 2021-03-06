﻿using System.Xml.Linq;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class PhoneApplicationManifestHelper
    {
        public static string GetVersion()
        {
            return XDocument.Load("WMAppManifest.xml")
                .Root.Element("App").Attribute("Version").Value;
        }

        public static string GetProductId()
        {
            return XDocument.Load("WMAppManifest.xml")
                .Root.Element("App").Attribute("ProductID").Value;
        }
    }
}
