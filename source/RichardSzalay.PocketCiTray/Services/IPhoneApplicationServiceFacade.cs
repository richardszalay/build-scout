using System;
using Microsoft.Phone.Shell;
namespace RichardSzalay.PocketCiTray.Services

{
    public interface IPhoneApplicationServiceFacade
    {
        IdleDetectionMode ApplicationIdleDetectionMode { get; set; }
    }
}
