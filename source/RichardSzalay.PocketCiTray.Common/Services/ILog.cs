using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ILog
    {
        ILog Write(string message, Exception ex);

        ILog Write(string format, params object[] args);
    }

    public interface ILogManager
    {
        ILogManager Disable();

        ILogManager Enable();
    }

    public class LoggingException : Exception
    {
        public LoggingException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
