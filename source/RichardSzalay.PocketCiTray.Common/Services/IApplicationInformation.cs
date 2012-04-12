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
        string ProductId { get; }
        bool IsTrialMode { get; }
    }

    public class ApplicationInformation : IApplicationInformation
    {
        public string ApplicationVersion
        {
            get { return PhoneApplicationManifestHelper.GetVersion(); }
        }

        public string ProductId
        {
            get { return PhoneApplicationManifestHelper.GetProductId(); }
        }

        public bool IsTrialMode
        {
            get
            {
#if DEBUG_TRIAL
                return true;
#else
                return new LicenseInformation().IsTrial();
#endif
            }
        }
    }
}
