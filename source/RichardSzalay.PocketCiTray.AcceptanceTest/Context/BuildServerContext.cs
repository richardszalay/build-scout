using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    /// <summary>
    /// Helps cross-functional steps keep context in their step phrases
    /// </summary>
    public class BuildServerContext : IDisposable
    {
        private List<BuilderServerStub> buildServers = new List<BuilderServerStub>();

        public ICollection<BuilderServerStub> BuildServers
        {
            get { return buildServers; }
        }

        public BuilderServerStub CurrentBuildServer
        {
            get { return buildServers.LastOrDefault(); }
        }

        public JobBuilder CurrentJob
        {
            get { return CurrentBuildServer.Jobs.LastOrDefault(); }
        }
        public void Dispose()
        {
            foreach (var buildServer in buildServers)
            {
                buildServer.Dispose();
            }
        }
    }
}
