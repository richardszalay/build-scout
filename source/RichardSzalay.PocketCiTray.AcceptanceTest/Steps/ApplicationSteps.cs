using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.PocketCiTray.AcceptanceTest.ApplicationDriver;
using TechTalk.SpecFlow;
using WindowsPhoneTestFramework.Server.AutomationController.WindowsPhone.Emulator;
using WindowsPhoneTestFramework.Server.Core.Gestures;
using WindowsPhoneTestFramework.Test.EmuSteps;
using WindowsPhoneTestFramework.Server.Core;
using RichardSzalay.PocketCiTray.AcceptanceTest.Context;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Steps
{
    [Binding]
    public class ApplicationSteps : EmuDefinitionBase
    {
        private WindowsPhoneTestFramework.Server.Core.IAutomationController automationController;
        private BuildServerContext buildContext;

        public ApplicationSteps(BuildServerContext buildContext)
        {
            this.buildContext = buildContext;
        }
        
        [BeforeScenario]
        public void SetupEmulator()
        {
            this.buildContext = buildContext;


        }

        [When(@"I add a new build server ""([^""]+)""")]
        public void CreateBuildServer(string buildServer)
        {
            var addServerDriver =new ListJobsDriver(Emu).AddFirstBuildServer();

            addServerDriver.AddBuildServer(buildContext.CurrentBuildServer);
        }

        [When(@"I add the ""([^""]+)"" job")]
        public void WhenIAddTheJob(string job)
        {
            
        }

    }
}
