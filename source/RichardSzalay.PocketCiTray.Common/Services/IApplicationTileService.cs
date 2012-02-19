using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationTileService
    {
        void AddJobTile(Job job);

        void UpdateAll(ICollection<Job> jobs);

        bool IsPinned(Job j);
    }
}
