using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace RichardSzalay.PocketCiTray.Data
{
    [Table(Name = "Job")]
    internal class JobEntity : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int id;
        private string alias;
        private DateTimeOffset? lastUpdated;
        private string name;
        private int notificationPreference;
        private string remoteId;
        private string webUri;
        private string lastBuildLabel;
        private int lastBuildResult;
        private DateTimeOffset lastBuildTime;
        private int buildServerId;
        private EntityRef<BuildServerEntity> buildServer;

        [Column(IsPrimaryKey =  true, IsDbGenerated = true, DbType = "INT NOT NULL IDENTITY", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    NotifyPropertyChanging("Id");
                    id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        [Column]
        public DateTimeOffset? LastUpdated
        {
            get { return lastUpdated; }
            set
            {
                if (value != lastUpdated)
                {
                    NotifyPropertyChanging("LastUpdated");
                    lastUpdated = value;
                    NotifyPropertyChanged("LastUpdated");
                }
            }
        }

        [Column(CanBeNull = true)]
        public string RemoteId
        {
            get { return remoteId; }
            set
            {
                if (value != remoteId)
                {
                    NotifyPropertyChanging("RemoteId");
                    remoteId = value;
                    NotifyPropertyChanged("RemoteId");
                }
            }
        }

        [Column]
        public int NotificationPreference
        {
            get { return notificationPreference; }
            set
            {
                if (value != notificationPreference)
                {
                    NotifyPropertyChanging("NotificationPreference");
                    notificationPreference = value;
                    NotifyPropertyChanged("NotificationPreference");
                }
            }
        }

        [Column(CanBeNull = true)]
        public string WebUri
        {
            get { return webUri; }
            set
            {
                if (value != webUri)
                {
                    NotifyPropertyChanging("WebUri");
                    webUri = value;
                    NotifyPropertyChanged("WebUri");
                }
            }
        }

        [Column(CanBeNull = true)]
        public string Alias
        {
            get { return alias; }
            set
            {
                if (value != alias)
                {
                    NotifyPropertyChanging("Alias");
                    alias = value;
                    NotifyPropertyChanged("Alias");
                }
            }
        }

        [Column]
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    NotifyPropertyChanging("Name");
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        [Column(CanBeNull = true)]
        public string LastBuildLabel
        {
            get { return lastBuildLabel; }
            set
            {
                if (value != lastBuildLabel)
                {
                    NotifyPropertyChanging("LastBuildLabel");
                    lastBuildLabel = value;
                    NotifyPropertyChanged("LastBuildLabel");
                }
            }
        }

        [Column]
        public int LastBuildResult
        {
            get { return lastBuildResult; }
            set
            {
                if (value != lastBuildResult)
                {
                    NotifyPropertyChanging("LastBuildResult");
                    lastBuildResult = value;
                    NotifyPropertyChanged("LastBuildResult");
                }
            }
        }

        [Column]
        public DateTimeOffset LastBuildTime
        {
            get { return lastBuildTime; }
            set
            {
                if (value != lastBuildTime)
                {
                    NotifyPropertyChanging("LastBuildTime");
                    lastBuildTime = value;
                    NotifyPropertyChanged("LastBuildTime");
                }
            }
        }

        [Column]
        public int BuildServerId
        {
            get { return buildServerId; }
            set
            {
                if (value != buildServerId)
                {
                    NotifyPropertyChanging("BuildServerId");
                    buildServerId = value;
                    NotifyPropertyChanged("BuildServerId");
                }
            }
        }

        

        [Column]
        [Association(ThisKey = "BuildServerId", OtherKey = "Id")]
        public BuildServerEntity BuildServer
        {
            get { return buildServer.Entity; }
            set
            {
                if (value != buildServer.Entity)
                {
                    NotifyPropertyChanging("BuildServer");
                    buildServer.Entity = value;
                    NotifyPropertyChanged("BuildServer");
                }
            }
        }

        [Column(IsVersion = true)]
        private Binary version;

        

        public void Update(Job job)
        {
            Name = job.Name;
            Alias = job.Alias;
            WebUri = job.WebUri.AbsoluteUri;
            NotificationPreference = (int) job.NotificationPreference;
            RemoteId = job.RemoteId;
            LastUpdated = job.LastUpdated;
            LastBuildLabel = job.LastBuild.Label;
            LastBuildResult = (int)job.LastBuild.Result;
            LastBuildTime = job.LastBuild.Time;
        }

        public Job ToJob(BuildServer actualBuildServer)
        {
            return new Job
            {
                Id = id,
                Name = name,
                Alias = alias,
                RemoteId = remoteId,
                NotificationPreference = (NotificationReason) NotificationPreference,
                WebUri = (WebUri == null)
                             ? null
                             : new Uri(WebUri, UriKind.Absolute),
                LastUpdated = lastUpdated,
                LastBuild = new Build
                {
                    Label = lastBuildLabel,
                    Result = (BuildResult)lastBuildResult,
                    Time = lastBuildTime
                },
                BuildServer = actualBuildServer
            };
        }

        public static JobEntity FromJob(Job job)
        {
            var entity = new JobEntity()
            {
                BuildServerId = job.BuildServer.Id
            };

            entity.Update(job);

            return entity;
        }

        private void NotifyPropertyChanged(string property)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        private void NotifyPropertyChanging(string property)
        {
            var handler = PropertyChanging;

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
    }
}