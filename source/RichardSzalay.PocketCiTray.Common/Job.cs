using System;

namespace RichardSzalay.PocketCiTray
{
    public class Job
    {
        public Job()
        {
            this.LastBuild = new Build
            {
                Result = BuildResult.Unavailable,
                Time = DateTimeOffset.MinValue
            };
        }

        public string Name { get; set; }

        public string Alias { get; set; }

        public Uri WebUri { get; set; }

        public BuildServer BuildServer { get; set; }

        public int Id { get; set; }

        public Build LastBuild { get; set; }

        public string DisplayName
        {
            get { return String.IsNullOrEmpty(Alias) ? Name : Alias; }
        }

        public override bool Equals(object obj)
        {
            Job otherJob = obj as Job;

            if (otherJob == null)
            {
                return false;
            }

            return this.BuildServer.Equals(otherJob.BuildServer) &&
                this.Name == otherJob.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                BuildServer.GetHashCode();
        }
    }
}