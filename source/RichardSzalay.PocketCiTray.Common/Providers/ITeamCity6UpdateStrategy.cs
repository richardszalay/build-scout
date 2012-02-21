using System;
using System.Collections.Generic;
namespace RichardSzalay.PocketCiTray.Providers
{
    interface ITeamCity6UpdateStrategy
    {
        IObservable<Job> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs);
    }
}
