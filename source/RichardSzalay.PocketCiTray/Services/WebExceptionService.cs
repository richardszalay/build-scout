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

namespace RichardSzalay.PocketCiTray.Services
{
    public class WebExceptionService
    {
        public static string GetDisplayMessage(WebException ex)
        {
            switch (ex.Status)
            {
                case WebExceptionStatus.UnknownError:
                case WebExceptionStatus.ProtocolError:
                    return String.Format(Strings.HttpServerResponseStatusError, ((HttpWebResponse)ex.Response).StatusCode);

                case WebExceptionStatus.Timeout:
                    return Strings.HttpServerResponseTimedOutError;

                case WebExceptionStatus.SecureChannelFailure:
                case WebExceptionStatus.TrustFailure:
                    return Strings.HttpServerResponseSslError;

                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return Strings.HttpServerResponseConnection;

                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.PipelineFailure:
                    return Strings.HttpServerResponseNetwork;

                default:
                    return Strings.HttpServerUnexpectedResponse;
            }
        }
    }
}
