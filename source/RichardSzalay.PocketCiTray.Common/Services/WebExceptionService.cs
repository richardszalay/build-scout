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

        public static bool IsJobUnavailable(WebException ex)
        {
            return unavailableIndicatingStatuses.ContainsKey(ex.Status);
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
