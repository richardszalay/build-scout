using System;
using System.Net;
using System.Reactive.Linq;
using System.Xml.Linq;

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
            request.Accept = "text/xml";
            request.Credentials = credentials;

            return request;
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