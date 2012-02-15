using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    public abstract class BuilderServerStub : IDisposable
    {
        private readonly Uri uri;

        protected BuilderServerStub(string name, Uri uri)
        {
            this.uri = uri;
            this.Name = name;
        }

        private List<JobBuilder> jobs = new List<JobBuilder>();

        public Uri BaseUri
        {
            get { return uri; }
        }

        public string Name { get; private set; }
        public List<JobBuilder> Jobs { get { return jobs; } }

        public abstract void Dispose();

    }
}
