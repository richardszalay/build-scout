using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Services
{
    public class WebExceptionService
    {
        private static readonly Dictionary<WebExceptionStatus, object> unavailableIndicatingStatuses = new Dictionary<WebExceptionStatus, object>
        {
            { WebExceptionStatus.UnknownError, null },
            { WebExceptionStatus.ProtocolError, null },
            { WebExceptionStatus.ConnectFailure, null }
        };

        public static bool IsJobUnavailable(Exception ex)
        {
            var wex = ex as WebException;

            return ex is TimeoutException ||
                (wex != null && unavailableIndicatingStatuses.ContainsKey(wex.Status));
        }

        public static string GetDebugMessage(Exception ex)
        {
            return ex is WebException
                ? GetDebugMessage((WebException) ex)
                : ex.Message;
        }

        public static string GetDebugMessage(WebException ex)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WebException: ");
            sb.Append(ex.Status.ToString("G"));

            HttpWebResponse httpResponse = ex.Response as HttpWebResponse;

            if (httpResponse != null)
            {
                sb.AppendFormat(" - {0} {1}", (int) httpResponse.StatusCode, httpResponse.StatusDescription);

#if DEBUG
                using (var stream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    sb.AppendFormat(reader.ReadToEnd());
                }
#endif
            }

            return sb.ToString();
        }

        public static string GetDisplayMessage(Exception ex)
        {
            if (ex is TimeoutException)
            {
                return CommonStrings.HttpServerResponseTimedOutError;
            }
            
            return (ex is WebException)
                ? GetDisplayMessage((WebException)ex)
                : "";
        }

        public static string GetDisplayMessage(WebException ex)
        {
            switch (ex.Status)
            {
                case WebExceptionStatus.UnknownError:
                case WebExceptionStatus.ProtocolError:
                    return String.Format(CommonStrings.HttpServerResponseStatusError, ((HttpWebResponse)ex.Response).StatusCode);

                case WebExceptionStatus.Timeout:
                    return CommonStrings.HttpServerResponseTimedOutError;

                case WebExceptionStatus.SecureChannelFailure:
                case WebExceptionStatus.TrustFailure:
                    return CommonStrings.HttpServerResponseSslError;

                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return CommonStrings.HttpServerResponseConnection;

                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.PipelineFailure:
                    return CommonStrings.HttpServerResponseNetwork;

                default:
                    return CommonStrings.HttpServerUnexpectedResponse;
            }
        }
    }
}
