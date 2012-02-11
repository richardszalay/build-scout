using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using System.Threading;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Steps
{
    [Binding]
    public class BuildResultSteps
    {
        [Given(@"I have previously added the job")]
        public void GivenIHavePreviouslyAddedTheJob()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I view the status of my jobs")]
        public void WhenIViewTheStatusOfMyJobs()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the result should indicate the job was (successful)")]
        [Then(@"the result should indicate the job (failed)")]
        [Then(@"the result should indicate the job is (unavailable)")]
        public void ThenTheResultShouldIndicateTheJobsBuildResultWas(string status)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the result should include the jobs name")]
        public void ThenTheResultShouldIncludeTheJobsName()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the result should include the last run time of the job")]
        public void ThenTheResultShouldIncludeTheLastRunTimeOfTheJob()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
