using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Xml.Linq;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class CruiseProvider : IJobProvider
    {
        public const string ProviderName = "cruise";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;

        public CruiseProvider(IWebRequestCreate webRequestCreate, IClock clock)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
        }

        public string Name { get { return ProviderName; } }

        public JobProviderFeature Features { get { return JobProviderFeature.JobDiscoveryIncludesStatus; } }

        public IObservable<ICollection<Job>> GetJobsObservableAsync(BuildServer buildServer)
        {
            var request = (HttpWebRequest)webRequestCreate.Create(buildServer.Uri);
            request.Accept = "text/xml";

            return request.GetResponseObservable()
                .Select(response =>
                {
                    using (var stream = response.GetResponseStream())
                        return MapJobs(buildServer, XDocument.Load(stream));
                });
        }

        public IObservable<BuildServer> ValidateBuildServer(BuildServer buildServer)
        {
            var request = (HttpWebRequest)webRequestCreate.Create(buildServer.Uri);
            request.Accept = "text/xml";
            request.Credentials = buildServer.Credential;

            return request.GetResponseObservable()
                .Select(response =>
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (XDocument.Load(stream).Root.Name == "Projects")
                        {
                            return buildServer;
                        }

                        throw new InvalidOperationException("Invalid build server");
                    }

                    response.Close();
                })
                .Catch<BuildServer, WebException>(ex =>
                {
                    var response = (HttpWebResponse) ex.Response;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return Observable.Throw<BuildServer>(new UnauthorizedAccessException());
                    }

                    return Observable.Throw<BuildServer>(ex);
                });
        }

        public IObservable<Job> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs)
        {
            return GetJobsObservableAsync(buildServer)
                .SelectMany(latestJobs =>
                {
                    var latestJobMap = latestJobs.ToDictionary(j => j.Name);

                    foreach(var job in jobs)
                    {
                        if (latestJobMap.ContainsKey(job.Name))
                        {
                            job.LastBuild = latestJobMap[job.Name].LastBuild;
                        }
                        else
                        {
                            job.LastBuild = new Build()
                            {
                                Result = BuildResult.Unavailable,
                                Time = clock.UtcNow
                            };
                        }
                    }

                    return (ICollection<Job>)jobs.ToList();
                });
        }

        private ICollection<Job> MapJobs(BuildServer buildServer, XDocument cruiseXml)
        {
            return cruiseXml.Root.Elements("Project")
                .Select(project => new Job
                {
                    BuildServer = buildServer,
                    Name = project.Attribute("name").Value,
                    WebUri = new Uri(project.Attribute("webUrl").Value),
                    LastBuild = new Build
                    {
                        Result = ParseBuildResult(project.Attribute("lastBuildStatus").Value),
                        Time = ParseBuildTime(project.Attribute("lastBuildTime").Value),
                        Label = project.Attribute("lastBuildLabel").Value
                    }
                })
                .ToList();
        }

        private static DateTimeOffset ParseBuildTime(string value)
        {
            return DateTimeOffset.Parse(value);
        }

        private static BuildResult ParseBuildResult(string value)
        {
            switch(value)
            {
                case "Success":
                    return BuildResult.Success;
                case "Failure":
                case "Error":
                    return BuildResult.Failed;
                default:
                    return BuildResult.Unavailable;
            }
        }
    }
}
