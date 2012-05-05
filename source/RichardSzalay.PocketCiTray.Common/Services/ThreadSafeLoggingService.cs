using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Environment = Microsoft.Devices.Environment;

namespace RichardSzalay.PocketCiTray.Services
{
    public sealed class ThreadSafeLoggingService : ILogManager, ILog, IDisposable
    {
        // Fields
        private readonly string applicationName;
        private volatile bool enabled;
        private const string FailedToClear = "Failed to clear log file.";
        private const string FailedToDisable = "Failed to disable logging.";
        private const string FailedToEnable = "Failed to enable logging.";
        private const string FailedToReadMessages = "Failed to read messages from file.";
        private const string FailedToWrite = "Failed to write to log.";
        private const string FailedToWriteDiagnostics = "Failed to write diagnostics to log.";
        private IsolatedStorageFile isolatedStorage;
        private const string LogFilename = "log.dat";
        private readonly string logPath;
        private const string MessageFormat = "{0:yyyy-MM-dd HH:mm:ss.ffff} - {1}";
        private readonly NumberFormatInfo numberFormatInfo;
        private readonly Queue<string> pendingMessages;
        private volatile bool persistingToFile;
        private readonly object sync;
        private const int WriteFrequency = 0x14d;
        private IDisposable writeObserver;

        // Events
        public event EventHandler LogModified;

        // Methods
        public ThreadSafeLoggingService(string applicationName)
        {
            var info = new NumberFormatInfo
            {
                NumberGroupSizes = new int[] { 3 },
                NumberGroupSeparator = ","
            };
            this.numberFormatInfo = info;
            this.pendingMessages = new Queue<string>();
            this.sync = new object();
            this.applicationName = applicationName;
            this.isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            string path = this.applicationName.ToLowerInvariant();
            this.isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!this.isolatedStorage.DirectoryExists(path))
            {
                this.isolatedStorage.CreateDirectory(path);
            }
            this.logPath = Path.Combine(path, LogFilename);
        }

        public ILogManager Clear()
        {
            ILogManager manager;
            try
            {
                lock (this.sync)
                {
                    this.pendingMessages.Clear();
                    this.ClearFile();
                }
                manager = this;
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToClear, exception);
            }
            return manager;
        }

        private void ClearFile()
        {
            try
            {
                new StreamWriter(isolatedStorage.CreateFile(LogPath))
                    .Dispose();
            }
            catch (Exception)
            {
            }
        }

        public ILogManager Disable()
        {
            ILogManager manager;
            try
            {
                if (!enabled)
                {
                    return this;
                }
                if (writeObserver != null)
                {
                    writeObserver.Dispose();
                    writeObserver = null;
                }
                PersistQueueToFile();
                enabled = false;
                manager = this;
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToDisable, exception);
            }
            return manager;
        }

        public void Dispose()
        {
            if (writeObserver != null)
            {
                writeObserver.Dispose();
                writeObserver = null;
                PersistQueueToFile();
                enabled = false;
            }
            if (isolatedStorage != null)
            {
                isolatedStorage.Dispose();
                isolatedStorage = null;
            }
        }

        public ILogManager Enable()
        {
            try
            {
                if (this.enabled)
                {
                    return this;
                }
                this.enabled = true;

                writeObserver = Observable.Interval(TimeSpan.FromMilliseconds(WriteFrequency))
                    .SubscribeOn(Scheduler.ThreadPool)
                    .Subscribe(_ => PersistQueueToFile());
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToEnable, exception);
            }
            return this;
        }

        private void Enqueue(List<string> messages)
        {
            lock (sync)
            {
                messages.ForEach(m => pendingMessages.Enqueue(m));
            }
        }

        private void PersistQueueToFile()
        {
            if (!persistingToFile)
            {
                lock (sync)
                {
                    persistingToFile = true;
                    var source = new List<string>();
                    while (pendingMessages.Count != 0)
                    {
                        source.Add(pendingMessages.Dequeue());
                    }
                    if (source.Count() != 0)
                    {
                        WriteFile(source);
                    }
                    persistingToFile = false;
                }
            }
        }

        private IEnumerable<string> ReadFile()
        {
            var list = new List<string>();
            try
            {
                using (var reader = new StreamReader(isolatedStorage.OpenFile(logPath, 
                    FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        public ILog Write(string message)
        {
            ILog log;
            try
            {
                string str = string.Format(MessageFormat, DateTime.Now, message);
                Enqueue(new List<string> { str });
                log = this;
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToWrite, exception);
            }
            return log;
        }

        public ILog Write(string message, params object[] args)
        {
            try
            {
                if (!enabled)
                {
                    return this;
                }

                Enqueue(new List<string>
                {
                    string.Format(MessageFormat, DateTime.Now, string.Format(message, args))
                });
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToWrite, exception);
            }
            return this;
        }

        public ILog Write(string message, Exception exception)
        {
            try
            {
                DateTime now = DateTime.Now;

                Enqueue(new List<string>
                {
                    String.Format(MessageFormat, now, message),
                    String.Format(MessageFormat, now, exception.Message),
                    String.Format(MessageFormat, now, exception)
                });
            }
            catch (Exception exception2)
            {
                throw new LoggingException(FailedToWrite, exception2);
            }
            return this;
        }

        public ILog WriteDiagnostics()
        {
            try
            {
                if (!enabled)
                {
                    return this;
                }
                DateTime now = DateTime.Now;

                string str = string.Format("Allocated memory - {0} bytes", GC.GetTotalMemory(true).ToString("N0", this.numberFormatInfo));
                string str2 = string.Format("Isolated storage free memory - {0} bytes", this.isolatedStorage.AvailableFreeSpace.ToString("N0", this.numberFormatInfo));
                string str3 = string.Format("Isolated storage quota - {0} bytes", this.isolatedStorage.Quota.ToString("N0", this.numberFormatInfo));
                
                var messages = new List<string> {
                    String.Format(MessageFormat, now, Environment.DeviceType),
                    String.Format(MessageFormat, now, str),
                    String.Format(MessageFormat, now, str2),
                    String.Format(MessageFormat, now, str3)
                };

                this.Enqueue(messages);
            }
            catch (Exception exception)
            {
                throw new LoggingException(FailedToWriteDiagnostics, exception);
            }
            return this;
        }

        private void WriteFile(List<string> messages)
        {
            try
            {
                using (var writer = new StreamWriter(isolatedStorage.OpenFile(logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)))
                {
                    messages.ForEach(writer.WriteLine);
                }

                var handler = LogModified;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch (Exception)
            {
            }
        }

        // Properties
        public string LogPath
        {
            get
            {
                return this.logPath;
            }
        }

        public IEnumerable<string> Messages
        {
            get
            {
                IEnumerable<string> enumerable;
                try
                {
                    enumerable = this.ReadFile();
                }
                catch (Exception exception)
                {
                    throw new LoggingException(FailedToReadMessages, exception);
                }
                return enumerable;
            }
        }
    }
}
