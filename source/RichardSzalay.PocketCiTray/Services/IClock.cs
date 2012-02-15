using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
