using System;
using System.Net;
using System.Reactive.Linq;
using System.Xml.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.Extensions.Extensions
{
    public static class WebRequestExtensions
    {
        public static IObservable<WebResponse> GetResponseObservable(this WebRequest request)
        {
            return Observable.FromAsyncPattern(
                request.BeginGetResponse,
                result => request.EndGetResponse(result)
                )()
                .Finally(() => request.Abort());
        }

        public static WebRequest CreateXmlRequest(this IWebRequestCreate creator, Uri uri, ICredentials credentials)
        {
            var request = (HttpWebRequest)creator.Create(uri);
            request.Accept = "text/xml, application/xml";
            request.Credentials = credentials;

            return request.FixUsernameHandling();
        }

        public static WebRequest CreateHtmlRequest(this IWebRequestCreate creator, Uri uri, ICredentials credentials)
        {
            var request = (HttpWebRequest)creator.Create(uri);
            request.Accept = "text/html";
            request.Credentials = credentials;

            return request.FixUsernameHandling();
        }

        public static WebRequest CreateTextRequest(this IWebRequestCreate creator, Uri uri, ICredentials credentials)
        {
            var request = (HttpWebRequest)creator.Create(uri);
            request.Accept = "text/plain";
            request.Credentials = credentials;

            return request.FixUsernameHandling();
        }

        public static WebRequest FixUsernameHandling(this WebRequest webRequest)
        {
            var credentials = webRequest.Credentials as NetworkCredential;

            if (credentials != null && !String.IsNullOrEmpty(credentials.UserName) && 
                String.IsNullOrEmpty(credentials.Password))
            {
                string authValue = "Basic " +
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials.UserName + ":"));

                webRequest.Credentials = null;
                webRequest.Headers[HttpRequestHeader.Authorization] = authValue;
            }

            return webRequest;
        }

        public static IObservable<XDocument> ParseXmlResponse(this IObservable<WebResponse> observableResponse)
        {
            return observableResponse
                .Select(response =>
                {
                    using (var stream = response.GetResponseStream())
                    {
                        return XDocument.Load(stream);
                    }
                });
        }
    }
}