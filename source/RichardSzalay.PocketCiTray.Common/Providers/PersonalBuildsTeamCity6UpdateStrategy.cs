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
    public class PersonalBuildsTeamCity6UpdateStrategy : RichardSzalay.PocketCiTray.Providers.ITeamCity6UpdateStrategy
    {
        private const string TeamCityDateFormat = "yyyyMMddThhmmsszzz";

        private readonly IWebRequestCreate webRequestCreate;
        private readonly IClock clock;
        private readonly ILog log;

        public PersonalBuildsTeamCity6UpdateStrategy(IWebRequestCreate webRequestCreate, IClock clock, ILog log)
        {
            this.webRequestCreate = webRequestCreate;
            this.clock = clock;
            this.log = log;
        }

        public IObservable<Job> UpdateAll (BuildServer buildServer, IEnumerable<Job> jobs)
        {
            log.Write("[PersonalBuildsTeamCity6UpdateStrategy] Updating jobs from server: {0}", buildServer.Uri);

            var personalBuildsUri = new Uri(buildServer.Uri, "/httpAuth/app/rest/6.0/builds?locator=personal:true");

            var indexedJobs = jobs.ToDictionary(x => x.RemoteId);

            return webRequestCreate.CreateXmlRequest(personalBuildsUri, buildServer.Credential)
                .GetResponseObservable()
                .ParseXmlResponse()
                .SelectMany(buildsDoc =>
                {
                    var updatedJobs = new List<Job>();

                    foreach (var buildElement in buildsDoc.Root.Elements("build"))
                    {
                        string buildTypeId = buildElement.Attribute("buildTypeId").Value;

                        Job job;

                        if (indexedJobs.TryGetValue(buildTypeId, out job))
                        {
                            job.LastBuild = MapBuild(buildElement);
                            job.LastUpdated = clock.UtcNow;

                            indexedJobs.Remove(buildTypeId);

                            updatedJobs.Add(job);
                        }
                    }

                    log.Write("[PersonalBuildsTeamCity6UpdateStrategy] Updated {0} jobs using recent builds list",
                        updatedJobs.Count);

                    return updatedJobs;
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

        private static DateTimeOffset ParseBuildTime(string value)
        {
            return DateTimeOffset.ParseExact(ConvertToDotNetCompatibleDateString(value),
                "yyyyMMddTHHmmsszzz", CultureInfo.InvariantCulture);
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

