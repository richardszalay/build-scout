using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    public class JobBuilder
    {
        public string Name { get; set; }
        public BuildResult LastResult { get; set; }
        public JobState State { get; set; }
    }
}
