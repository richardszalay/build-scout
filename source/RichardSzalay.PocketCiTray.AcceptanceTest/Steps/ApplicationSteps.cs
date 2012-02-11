using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using WindowsPhoneTestFramework.Server.AutomationController.WindowsPhone.Emulator;
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
        
        [BeforeScenario]
        public void SetupEmulator(BuildServerContext buildContext)
        {
            this.buildContext = buildContext;
        }

        [When(@"I add a new cruise compatible job ""([^""]+)"" on ""([^""]+)""")]
        public void CreateCruiseJob(string job, string buildServer)
        {
            
            //this.buildContext.BuildServers.First(x => 

            
        }
    }
}
