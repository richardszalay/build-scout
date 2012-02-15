using System;
using RichardSzalay.PocketCiTray.AcceptanceTest.Context;
using WindowsPhoneTestFramework.Server.Core;
using WindowsPhoneTestFramework.Server.Core.Tangibles;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.ApplicationDriver
{
    public class AddBuildServerDriver : PageDriver
    {
        private ApplicationBarDriver appBarDriver;

        public AddBuildServerDriver(IAutomationController emulator)
            : base(emulator)
        {
            appBarDriver = new ApplicationBarDriver(emulator);
        }

        public void AddBuildServer(BuilderServerStub currentBuildServer)
        {
            Emulator.ApplicationAutomationController.SetTextOnControl("BuildSource", currentBuildServer.BaseUri.AbsoluteUri);

            Emulator.DisplayInputController.SendKeyPress(KeyboardKeyCode.RETURN);
        }
    }
}