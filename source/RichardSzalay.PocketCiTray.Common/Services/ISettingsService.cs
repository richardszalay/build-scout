using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ISettingsService
    {
        IDictionary<string, object> GetSettings();
        void SaveSettings(IDictionary<string, object> modifiedValues);
    }

    public class IsolatedStorageFileSettingsService : ISettingsService
    {
        private static readonly TimeSpan SettingsAccessTimeout = TimeSpan.FromSeconds(20);

        private readonly ISettingsDictionarySerializer serializer;
        private readonly IIsolatedStorageFacade isolatedStorageFacade;
        private readonly IMutexService mutexService;
        private readonly ILog log;

        private const string SettingsFile = ".settings";

        public IsolatedStorageFileSettingsService(ISettingsDictionarySerializer serializer, IIsolatedStorageFacade isolatedStorageFacade,
            IMutexService mutexService, ILog log)
        {
            this.serializer = serializer;
            this.isolatedStorageFacade = isolatedStorageFacade;
            this.mutexService = mutexService;
            this.log = log;
        }

        public IDictionary<string, object> GetSettings()
        {
            var mutex = mutexService.GetOwned(MutexNames.Settings, SettingsAccessTimeout);

            try
            {
                if (!isolatedStorageFacade.FileExists(SettingsFile))
                {
                    return new Dictionary<string, object>();
                }

                using (var input = isolatedStorageFacade.OpenFile(SettingsFile))
                {
                    return serializer.Deserialize(input);
                }
            }
            catch(IOException exception)
            {
                log.Write("Error reading settings", exception);

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                return new Dictionary<string, object>();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void SaveSettings(IDictionary<string, object> modifiedValues)
        {
            var mutex = mutexService.GetOwned(MutexNames.Settings, SettingsAccessTimeout);

            try
            {
                var settings = GetSettings();

                MergeInto(modifiedValues, settings);

                using (var output = isolatedStorageFacade.CreateFile(SettingsFile))
                {
                    serializer.Serialize(settings, output);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private void MergeInto(IDictionary<string, object> source, IDictionary<string, object> target)
        {
            foreach(var kvp in source)
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }
}
