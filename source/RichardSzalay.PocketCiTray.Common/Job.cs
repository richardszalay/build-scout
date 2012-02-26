using System;

namespace RichardSzalay.PocketCiTray
{
    public class Job
    {
        public Job()
        {
            this.NotificationPreference = NotificationReason.All;

            this.lastBuild = new Build
            {
                Result = BuildResult.Unavailable,
                Time = DateTimeOffset.MinValue
            };
        }

        public string Name { get; set; }

        public string Alias { get; set; }

        public Uri WebUri { get; set; }

        public int Id { get; set; }

        public string RemoteId { get; set; }

        public BuildServer BuildServer { get; set; }
        
        private Build lastBuild;
        public Build LastBuild
        {
            get { return lastBuild; }
            set
            {
                value.Change = Build.GetBuildResultChange(lastBuild.Result, value.Result);

                lastBuild = value;
            }
        }

        public DateTimeOffset? LastUpdated { get; set; }

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

        public NotificationReason NotificationPreference { get; set; }
    }
}