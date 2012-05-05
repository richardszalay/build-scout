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
            var sb = new StringBuilder();

            sb.Append("WebException: ");

            if (IsIncorrectlyReportedConnectionError(ex))
            {
                sb.Append(WebExceptionStatus.ConnectFailure.ToString("G"));
                return sb.ToString();
            }

            sb.Append(ex.Status.ToString("G"));

            var httpResponse = ex.Response as HttpWebResponse;

            if (httpResponse != null)
            {
                sb.AppendFormat(" - {0} {1}", (int) httpResponse.StatusCode, httpResponse.StatusDescription);

#if DEBUG
                try
                {
                    using (var stream = httpResponse.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        sb.AppendFormat(": {0}", reader.ReadToEnd());
                    }
                }
                catch (Exception)
                {
                    sb.Append("Response not available");
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
            var status = IsIncorrectlyReportedConnectionError(ex)
                ? WebExceptionStatus.ConnectFailure
                : ex.Status;

            switch (status)
            {
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
                    var response = ex.Response as HttpWebResponse;

                    return (response != null)
                        ? GetDisplayMessage(response)
                        : CommonStrings.HttpServerUnexpectedResponse;
            }
        }

        public static string GetDisplayMessage(HttpWebResponse response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.RequestTimeout:
                    return CommonStrings.HttpServerResponseTimedOutError;

                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Unauthorized:
                    return CommonStrings.HttpServerResponseForbidden;

                default:
                    string statusLine = String.Format("{0:D} {1}",
                        response.StatusCode, response.StatusDescription);

                    return String.Format(CommonStrings.HttpServerHttpStatusError, statusLine);
            }

            


        }

        private static bool IsIncorrectlyReportedConnectionError(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;

            return ex.Status == WebExceptionStatus.UnknownError &&
                   response != null &&
                   response.StatusCode == HttpStatusCode.NotFound &&
                   response.ResponseUri.OriginalString == "";
        }
    }
}
