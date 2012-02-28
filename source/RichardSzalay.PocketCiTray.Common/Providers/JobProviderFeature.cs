using System;

namespace RichardSzalay.PocketCiTray.Providers
{
    [Flags]
    public enum JobProviderFeature
    {
        None = 0,
        JobDiscoveryIncludesStatus = 1
    }
}
