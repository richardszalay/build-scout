using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Collections.Generic;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class StubLoggingService : ILogManager, ILog
    {
        private List<string> loggedMessages = new List<string>();
        private List<Exception> loggedExceptions = new List<Exception>();

        public List<string> LoggedMessages
        {
            get { return loggedMessages; }
        }

        public List<Exception> LoggedExceptions
        {
            get { return loggedExceptions; }
        }

        public ILogManager Clear()
        {
            loggedMessages.Clear();
            loggedExceptions.Clear();
            return this;
        }

        public ILogManager Disable()
        {
            IsEnabled = false;
            return this;
        }

        public ILogManager Enable()
        {
            IsEnabled = true;
            return this;
        }

        public event EventHandler LogModified
        {
            add { throw new NotSupportedException(); }
            remove { throw new NotSupportedException(); }
        }

        public string LogPath
        {
            get { return "stub"; }
        }

        public System.Collections.Generic.IEnumerable<string> Messages
        {
            get { return loggedMessages; }
        }

        public ILog Write(string message, Exception exception)
        {
            Write(message);
            loggedExceptions.Add(exception);
            return this;
        }

        public ILog Write(string message, params object[] args)
        {
            Write(String.Format(message, args));
            return this;
        }

        public ILog Write(string message)
        {
            this.loggedMessages.Add(message);
            return this;
        }

        public ILog WriteDiagnostics()
        {
            Write("diagnostics");
            return this;
        }

        public bool IsEnabled { get; set; }
    }
}
