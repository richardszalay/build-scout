using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Steps
{
    public class StepUtilities
    {
        public static BuildResult ParseBuildResult(string status)
        {
            return (BuildResult)Enum.Parse(typeof(BuildResult), status, true);
        }
    }
}
