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
using System.Reactive.Concurrency;
using System.Reactive;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class TeamCity6Provider : IJobProvider
    {
        /// <summary>
        /// 6.x compatible
        /// </summary>
        public const string ProviderName = "teamcity6";

        /// <summary>
        /// 7.0+ compatible with cruise
        /// </summary>
        public const string CruiseProviderName = "cruise-teamcity";

        private const string TeamCityDateFormat = "yyyyMMddThhmmsszzz";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;
        private readonly ILog log;

        private IEnumerable<ITeamCity6UpdateStrategy> updateStrategies;

        public TeamCity6Provider(IWebRequestCreate webRequestCreate, IClock clock, ILog log)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
            this.log = log;

            updateStrategies = new ITeamCity6UpdateStrategy[]
            {
                new PersonalBuildsTeamCity6UpdateStrategy(webRequestCreate, clock, log),
                new PerJobTeamCity6UpdateStrategy(webRequestCreate, clock, log)
            };
        }

        public string Name { get { return ProviderName; } }

        public JobProviderFeature Features { get { return JobProviderFeature.None; } }

        public IObservable<ICollection<Job>> GetJobsObservableAsync(BuildServer buildServer)
        {
            Uri jobsUri = new Uri(buildServer.Uri, "httpAuth/app/rest/buildTypes");
            var request = webRequestCreate.CreateXmlRequest(jobsUri, buildServer.Credential);

            var buildTypes = request.GetResponseObservable()
                .ParseXmlResponse()
                .Select(doc => (ICollection<Job>)MapJobs(doc, buildServer).ToList());

            return buildTypes;
        }

        public IObservable<BuildServer> ValidateBuildServer(BuildServer buildServer)
        {
            var validateUri = new Uri(buildServer.Uri, "httpAuth/app/rest/6.0/version");

            log.Write("[TeamCity6Provider] Validating server: {0}", validateUri);

            var request = webRequestCreate.CreateTextRequest(validateUri, buildServer.Credential);
            
            return request.GetResponseObservable()
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

            var remainingJobsSet = jobs.ToDictionary(j => j);

            return this.updateStrategies.Select(strat => Observable.Defer(() =>
            {
                return remainingJobsSet.Count > 0
                    ? strat.UpdateAll(buildServer, remainingJobsSet.Keys.ToList())
                        .Do(j => { if (remainingJobsSet.ContainsKey(j)) remainingJobsSet.Remove(j); })
                    : Observable.Empty<Job>();
            }))
            .Concat();
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

        private IEnumerable<Job> MapJobs(XDocument document, BuildServer buildServer)
        {
            return document.Root.Elements("buildType")
                .Select(jobElement => new Job
                {
                    // TODO: It would be nice to use @projectName here somehow
                    RemoteId = jobElement.Attribute("id").Value,
                    Name = jobElement.Attribute("name").Value,
                    WebUri = new Uri(jobElement.Attribute("webUrl").Value),
                    BuildServer = buildServer
                });
        }

        private Build MapBuild(XElement jobElement)
        {
            return new Build
            {
                Label = jobElement.Attribute("number").Value,
                Result = ParseBuildResult(jobElement.Attribute("status").Value),
                Time = ParseBuildTime(jobElement.Attribute("startDate").Value)
            };
        }

        private ICollection<string> MapBuildTypes(XDocument doc)
        {
            return doc.Root.Elements("build")
                .Select(b => b.Attribute("buildTypeId").Value)
                .ToList();
        }

        private static DateTimeOffset ParseBuildTime(string value)
        {
            return DateTimeOffset.Parse(value);
        }

        private static BuildResult ParseBuildResult(string value)
        {
            switch (value)
            {
                case "SUCCESS":
                    return BuildResult.Success;
                case "FAILURE":
                case "ERROR":
                    return BuildResult.Failed;
                default:
                    return BuildResult.Unavailable;
            }
        }
    }
}

