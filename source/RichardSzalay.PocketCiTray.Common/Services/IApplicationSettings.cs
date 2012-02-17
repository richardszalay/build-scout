using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationSettings
    {
        TimeSpan ApplicationUpdateInterval { get; set; }
        TimeSpan BackgroundUpdateInterval { get; set; }
        bool BackgroundUpdateEnabled { get; set; }
        bool FirstRun { get; set; }
        event EventHandler Updated;
        void Save();
    }
}