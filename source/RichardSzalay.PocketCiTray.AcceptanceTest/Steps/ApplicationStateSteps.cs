using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using RichardSzalay.PocketCiTray.AcceptanceTest.Context;
using System.Threading;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Steps
{
    [Binding]
    public class ApplicationStateSteps
    {
        private BuildServerContext context;

        public ApplicationStateSteps(BuildServerContext context)
        {
            this.context = context;
        }

        [Given(@"I am monitoring the job")]
        public void GivenIAmMonitoringTheJob()
        {
            
        }
    }
}
