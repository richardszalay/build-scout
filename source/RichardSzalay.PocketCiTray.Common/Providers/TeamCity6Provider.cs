using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Linq;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using System.IO;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class TeamCity6Provider : IJobProvider
    {
        /// <summary>
        /// 6.x compatible
        /// </summary>
        public const string ProviderName = "teamcity";

        /// <summary>
        /// 7.0+ compatible with cruise
        /// </summary>
        public const string CruiseProviderName = "cruise-teamcity";

        private const string TeamCityDateFormat = "yyyyMMddThhmmsszzz";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;
        private readonly ILog log;

        public TeamCity6Provider(IWebRequestCreate webRequestCreate, IClock clock, ILog log)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
            this.log = log;
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
            var validateUri = new Uri(buildServer.Uri, ApiSuffix + "/httpAuth/app/rest/6.0/version");

            log.Write("[TeamCity6Provider] Validating server: {0}", validateUri);

            return webRequestCreate.CreateXmlRequest(buildServer.Uri, buildServer.Credential)
                .GetResponseObservable()
                .Select(response =>
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string contentType = response.Headers[HttpRequestHeader.ContentType];

                        if (contentType != "text/plain")
                        {
                            throw new InvalidOperationException("Expected text/plain response from version service");
                        }

                        int version;
                        if (!Int32.TryParse(reader.ReadToEnd(), out version))
                        {
                            throw new InvalidOperationException("Invalid version response");
                        }

                        log.Write("[TeamCity6Provider] Validated Team City 6 server (REST API version: {0})", version);

                        return buildServer;
                    }
                });
        }

        public IObservable<Job> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs)
        {
            log.Write("[TeamCity6Provider] Updating jobs from server: {0}", buildServer.Uri);

            var oldestUpdateTime = GetOldestUpdate(jobs);

            var firstPageBuilder = new UriBuilder(new Uri(buildServer.Uri, "/httpAuth/app/rest/6.0/builds"));

            if (oldestUpdateTime.HasValue)
            {
                firstPageBuilder.Query = "sinceDate=" + 
                    Uri.EscapeUriString(FormatTeamCityDate(oldestUpdateTime.Value));
            }

            var remainingJobs = new Dictionary<string, Job>();

            Subject<Uri> pages = new Subject<Uri>();

            Func<Uri, IObservable<Job>> loadPage = uri => Observable.Defer(() => 
                webRequestCreate.CreateXmlRequest(uri, buildServer.Credential)
                    .GetResponseObservable()
                    .ParseXmlResponse()
                    .Select(buildsDoc =>
                    {
                        foreach (var buildElement in buildsDoc.Root.Elements("build"))
                        {
                            string buildTypeId = buildElement.Attribute("buildTypeId").Value;

                            Job job;

                            if (remainingJobs.TryGetValue(buildTypeId, out job))
                            {
                                remainingJobs.Remove()
                            }
                        }

                        XAttribute nextHrefAttr = buildsDoc.Root.Attribute("nextHref");
                        if (nextHrefAttr != null)
                        {
                            var nextPageUri = new Uri(buildServer.Uri, nextHrefAttr.Value);
                            pages.OnNext(nextPageUri);
                        }
                    });

            var loadEachPage = pages
                .StartWith(firstPageBuilder.Uri)
                .Select(loadPage)
                .Concat();


                
            return pages.StartWith(firstPageBuilder.Uri)

                .SelectMany(uri => webRequestCreate.CreateXmlRequest(firstPageBuilder.Uri, buildServer.Credential))
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

        private string FormatTeamCityDate(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(TeamCityDateFormat)
                .Replace(":", "");
        }

        private DateTimeOffset? GetOldestUpdate(IEnumerable<Job> jobs)
        {
            return jobs.Select(j => j.LastBuild)
                .OrderBy(b => b == null ? DateTimeOffset.MinValue : b.Time)
                .Select(b => b == null ? null : (DateTimeOffset?)b.Time)
                .FirstOrDefault();
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
