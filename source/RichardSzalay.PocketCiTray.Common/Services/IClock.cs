using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
