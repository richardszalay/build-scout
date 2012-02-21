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
using System.Globalization;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class PerJobTeamCity6UpdateStrategy : ITeamCity6UpdateStrategy
    {
        private const string TeamCityDateFormat = "yyyyMMddThhmmsszzz";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;
        private readonly ILog log;

        public PerJobTeamCity6UpdateStrategy(IWebRequestCreate webRequestCreate, IClock clock, ILog log)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
            this.log = log;
        }

        public IObservable<Job> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs)
        {
            log.Write("[PerJobTeamCity6UpdateStrategy] Updating jobs from server: {0}", buildServer.Uri);

            return PrioritiseJobUpdates(jobs)
                .Select(j => Observable.Return<Job>(j))
                .ToObservable()
                .Merge(2)
                .SelectMany(job =>
                {
                    string relativePath = String.Format("/httpAuth/app/rest/builds?locator=buildType:(id:{0}),count:1",
                        job.RemoteId);

                    var latestBuildUri = new Uri(buildServer.Uri, relativePath);

                    return webRequestCreate.CreateXmlRequest(latestBuildUri, buildServer.Credential)
                        .GetResponseObservable()
                        .ParseXmlResponse()
                        .SelectMany(buildsDoc =>
                        {
                            XElement buildElement = buildsDoc.Root.Elements("build").FirstOrDefault();

                            if (buildElement == null)
                            {
                                return Observable.Empty<Job>();
                            }
                            else
                            {
                                job.LastBuild = MapBuild(buildElement);
                                job.LastUpdated = clock.UtcNow;
                                return Observable.Return(job);
                            }
                        });
                });
        }

        private IEnumerable<Job> PrioritiseJobUpdates(IEnumerable<Job> jobs)
        {
            return jobs.OrderBy(x => x.LastUpdated);
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

        private Build MapBuild(XElement jobElement)
        {
            return new Build
            {
                Label = jobElement.Attribute("number").Value,
                Result = ParseBuildResult(jobElement.Attribute("status").Value),
                Time = ParseBuildTime(jobElement.Attribute("startDate").Value)
            };
        }

        private static DateTimeOffset ParseBuildTime(string value)
        {
            return DateTimeOffset.ParseExact(ConvertToDotNetCompatibleDateString(value),
                "yyyyMMddThhmmsszzz", CultureInfo.InvariantCulture);
        }

        private static string ConvertToDotNetCompatibleDateString(string teamCityDateString)
        {
            return String.Concat(
                teamCityDateString.Substring(0, teamCityDateString.Length - 2),
                ":",
                teamCityDateString.Substring(teamCityDateString.Length - 2));
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

