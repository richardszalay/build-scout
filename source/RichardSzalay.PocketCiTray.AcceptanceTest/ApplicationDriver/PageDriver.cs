using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneTestFramework.Server.Core;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.ApplicationDriver
{
    public class PageDriver
    {
        private IAutomationController emulator;

        private static readonly TimeSpan UiStepTimeout = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan defaultUiTimeout = TimeSpan.FromSeconds(3);

        public PageDriver(IAutomationController emulator)
        {
            this.emulator = emulator;
        }

        protected IAutomationController Emulator
        {
            get { return emulator; }
        }

        protected void AssertWaitForText(string text)
        {
            AssertWaitForText(text, defaultUiTimeout);
        }

        protected void AssertWaitForText(string text, TimeSpan timeout)
        {
            TimeSpan total = TimeSpan.Zero;

            while (total < timeout && !Emulator.ApplicationAutomationController.WaitForText("build server", UiStepTimeout))
            {
                total += UiStepTimeout;
            }
            
            if (total >= timeout)
            {
                throw new InvalidOperationException("Timed out waiting for text: " + text);
            }
        }
    }
}
