using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using RichardSzalay.PocketCiTray.AcceptanceTest.Context;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Steps
{
    [Binding]
    public class BuildServerStateSteps
    {
        private BuildServerContext context;

        public BuildServerStateSteps(BuildServerContext context)
        {
            this.context = context;
        }

        [Given(@"I have a build server called ""([^""]+)""")]
        public void GivenIHaveABuildServer(string buildServer)
        {
            context.BuildServers.Add(new CruiseCompatibleBuildServerStub(buildServer));
        }

        [Given(@"the build server has a job called ""([^""]+)""")]
        public void GivenTheBuildServerHasAJob(string name)
        {
            context.CurrentBuildServer.Jobs.Add(new JobBuilder()
            {
                Name = name
            });
        }

        [Given(@"the job's last run (failed)")]
        [Given(@"the job's last run was (success)ful")]
        [Given(@"the job's last run is (unavailable)")]
        [Given(@"the build server is (unavailable)")]
        public void GivenTheJobHasAStatus(string status)
        {
            context.CurrentJob.LastResult = StepUtilities.ParseBuildResult(status);
        }

        [AfterScenario]
        public void DisposeScenario()
        {
            context.Dispose();
        }
    }
}
