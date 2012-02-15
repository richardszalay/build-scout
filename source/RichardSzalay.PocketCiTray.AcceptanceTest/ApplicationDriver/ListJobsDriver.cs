using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneTestFramework.Server.Core;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.ApplicationDriver
{
    public class ListJobsDriver : PageDriver
    {
        private ApplicationBarDriver appBarDriver;

        public ListJobsDriver(IAutomationController emulator) : base(emulator)
        {
            appBarDriver = new ApplicationBarDriver(emulator);
        }

        public AddBuildServerDriver AddFirstBuildServer()
        {
            appBarDriver.SelectIconButton(1, 0);
            AssertWaitForText("build server");

            appBarDriver.SelectIconButton(1, 0);
            AssertWaitForText("add server");

            return new AddBuildServerDriver(Emulator);
        }
    }
}
