using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    public abstract class BuilderServerStub : IDisposable
    {
        protected BuilderServerStub(string name)
        {
            this.Name = name;
        }

        private List<JobBuilder> jobs = new List<JobBuilder>();

        public string Name { get; private set; }
        public List<JobBuilder> Jobs { get { return jobs; } }

        public abstract void Dispose();

    }
}
