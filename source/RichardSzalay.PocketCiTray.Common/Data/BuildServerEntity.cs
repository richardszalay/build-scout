using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Net;
using System.Security.Cryptography;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Data
{
    public class BuildServerEntity
    {
        private int id;
        private byte[] credential;
        private string name;
        private string provider;
        private string uri;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL IDENTITY", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
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

        [Column(CanBeNull = true)]
        public byte[] Credential
        {
            get { return credential; }
            set
            {
                if (value != credential)
                {
                    NotifyPropertyChanging("Credential");
                    credential = value;
                    NotifyPropertyChanged("Credential");
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

        [Column]
        public string Provider
        {
            get { return provider; }
            set
            {
                if (value != provider)
                {
                    NotifyPropertyChanging("Provider");
                    provider = value;
                    NotifyPropertyChanged("Provider");
                }
            }
        }

        [Column]
        public string Uri
        {
            get { return uri; }
            set
            {
                if (value != uri)
                {
                    NotifyPropertyChanging("Uri");
                    uri = value;
                    NotifyPropertyChanged("Uri");
                }
            }
        }

        internal BuildServer ToBuildServer(ICredentialEncryptor credentialEncryptor)
        {
            return new BuildServer()
            {
                Id = Id,
                Credential = (Credential == null)
                                 ? null
                                 : credentialEncryptor.Decrypt(Credential),
                Name = Name,
                Provider = Provider,
                Uri = new Uri(Uri, UriKind.Absolute),
            };
        }



        internal static BuildServerEntity FromBuildServer(BuildServer buildServer, ICredentialEncryptor credentialEncryptor)
        {
            return new BuildServerEntity
            {
                Id = buildServer.Id,
                Credential = (buildServer.Credential == null)
                                 ? null
                                 : credentialEncryptor.Encrypt(buildServer.Credential),
                Name = buildServer.Name,
                Provider = buildServer.Provider,
                Uri = buildServer.Uri.AbsoluteUri,
            };
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
