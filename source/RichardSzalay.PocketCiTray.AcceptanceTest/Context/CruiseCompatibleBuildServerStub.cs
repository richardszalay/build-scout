using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Http;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System.ServiceModel;
using System.Net.Http;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    public class CruiseCompatibleBuildServerStub : BuilderServerStub
    {
        private HttpServiceHost serviceHost;
        private Uri baseUri;


        public CruiseCompatibleBuildServerStub(string name)
            : base(name)
        {
            baseUri = new Uri("http://localhost:4120/" + name + "/");

            this.serviceHost = new HttpServiceHost(
                new CruiseControlWebService(this), baseUri);

            serviceHost.Open();
        }

        public override void Dispose()
        {
            if (serviceHost.State == CommunicationState.Opened)
            {
                serviceHost.Close();
            }
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class CruiseControlWebService
        {
            private CruiseCompatibleBuildServerStub serverStub;

            public CruiseControlWebService(CruiseCompatibleBuildServerStub serverStub)
            {
                this.serverStub = serverStub;
            }

            [WebGet(UriTemplate = "cc.xml")]
            public HttpResponseMessage GetProjectData()
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(new XDocument(new XElement("Projects",
                        serverStub.Jobs.Select(CreateProjectXml))).ToString(), Encoding.UTF8, "text/xml")
                };
            }

            private XElement CreateProjectXml(JobBuilder job)
            {
                return new XElement("Project",
                    new XAttribute("name", job.Name),
                    new XAttribute("activity", ConvertToCruiseProjectActivity(job.State)),
                    new XAttribute("lastBuildStatus", ConvertToCruiseBuildStatus(job.LastResult)),
                    new XAttribute("lastBuildTime", "2005-09-28T10:30:34.6362160+01:00"),
                    new XAttribute("nextBuildTime", "2005-10-04T14:31:52.4509248+01:00"),
                    new XAttribute("webUrl", "http://mrtickle/ccnet/")
                    );
            }

            private string ConvertToCruiseBuildStatus(BuildResult result)
            {
                switch (result)
                {
                    case BuildResult.Success: return "Success";
                    case BuildResult.Failed: return "Failure";
                    default: return "Unknown";
                }
            }

            private string ConvertToCruiseProjectActivity(JobState state)
            {
                switch (state)
                {
                    case JobState.Building: return "Building";
                    default: return "Sleeping";
                }
            }
        }
    }
}
