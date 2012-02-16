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
        
        public CruiseCompatibleBuildServerStub(string name)
            : base(name, new Uri("http://localhost:8095/" + name + "/cc.xml"))
        {
            this.serviceHost = new HttpServiceHost(
                new CruiseControlWebService(this), BaseUri);

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

            [WebGet(UriTemplate = "")]
            public HttpResponseMessage GetProjectData()
            {
                var message = new HttpResponseMessage
                {
                    Content = new StringContent(new XDocument(new XElement("Projects",
                        serverStub.Jobs.Select(CreateProjectXml))).ToString(), Encoding.UTF8, "text/xml")
                };

                message.Headers.Add("Cache-Control", "no-cache");

                return message;
            }

            private XElement CreateProjectXml(JobBuilder job)
            {
                return new XElement("Project",
                    new XAttribute("name", job.Name),
                    new XAttribute("activity", "Sleeping"),
                    new XAttribute("lastBuildStatus", ConvertToCruiseBuildStatus(job.LastResult)),
                    new XAttribute("lastBuildTime", DateTimeOffset.Now.ToString("u")),
                    new XAttribute("nextBuildTime", "2005-10-04T14:31:52.4509248+01:00"),
                    new XAttribute("webUrl", "http://mrtickle/ccnet/")
                    );
            }

            private BuildResult GetRandomBuildResult()
            {
                Random rand = new Random();

                return (BuildResult)rand.Next(0, 2);
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
        }
    }
}
