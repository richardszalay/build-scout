using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneTestFramework.Server.Core;
using WindowsPhoneTestFramework.Server.Core.Gestures;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.ApplicationDriver
{
    public class ApplicationBarDriver
    {
        private readonly IAutomationController emulator;

        public ApplicationBarDriver(IAutomationController emulator)
        {
            this.emulator = emulator;
        }

        public void SelectIconButton(int iconCount, int index)
        {
            if (iconCount != 1 || index != 0)
                throw new NotImplementedException();

            emulator.DisplayInputController.DoGesture(new TapGesture(239, 762));
        }
    }
}
