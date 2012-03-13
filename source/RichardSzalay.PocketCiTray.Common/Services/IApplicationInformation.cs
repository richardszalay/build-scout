using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Marketplace;
using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationInformation
    {
        string ApplicationVersion { get; }
        bool IsTrialMode { get; }
    }

    public class ApplicationInformation : IApplicationInformation
    {
        public string ApplicationVersion
        {
            get { return PhoneApplicationManifestHelper.GetVersion(); }
        }

        public bool IsTrialMode
        {
            get { return new LicenseInformation().IsTrial(); }
        }
    }
}
