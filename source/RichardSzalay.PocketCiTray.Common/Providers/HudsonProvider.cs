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
    public class HudsonProvider : IJobProvider
    {
        public const string ProviderName = "hudson";

        private const string ApiSuffix = "api/xml";
        private const string JobQuery = "?tree=jobs[name,url,lastCompletedBuild[result,timestamp,number]]";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;

        public HudsonProvider(IWebRequestCreate webRequestCreate, IClock clock)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
        }

        public string Name { get { return ProviderName; } }

        public IObservable<ICollection<Job>> GetJobsObservableAsync(BuildServer buildServer)
        {
            Uri jobsUri = new Uri(buildServer.Uri, ApiSuffix + JobQuery);

            var request = (HttpWebRequest)webRequestCreate.Create(jobsUri);
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
            Uri validateUri = new Uri(buildServer.Uri, ApiSuffix + "?tree=hudson");

            var request = (HttpWebRequest)webRequestCreate.Create(validateUri);
            request.Accept = "text/xml";
            request.Credentials = buildServer.Credential;

            return request.GetResponseObservable()
                .Select(response =>
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (XDocument.Load(stream).Root.Name == "hudson")
                        {
                            return buildServer;
                        }

                        throw new InvalidOperationException("Invalid build server");
                    }
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
            return cruiseXml.Root.Elements("job")
                .Select(project => new Job
                {
                    BuildServer = buildServer,
                    Name = project.Element("name").Value,
                    WebUri = new Uri(project.Element("url").Value),
                    LastBuild = new Build
                    {
                        Result = ParseBuildResult(project.Element("lastCompletedBuild").Element("result").Value),
                        Time = ParseBuildTime(project.Element("lastCompletedBuild").Element("timestamp").Value),
                        Label = project.Element("lastCompletedBuild").Element("number").Value
                    }
                })
                .ToList();
        }

        private static DateTimeOffset ParseBuildTime(string value)
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)
                + TimeSpan.FromMilliseconds(Int64.Parse(value));
        }

        private static BuildResult ParseBuildResult(string value)
        {
            switch (value)
            {
                case "SUCCESS":
                    return BuildResult.Success;
                case "FAILURE":
                    return BuildResult.Failed;
                default:
                    return BuildResult.Unavailable;
            }
        }
    }
}