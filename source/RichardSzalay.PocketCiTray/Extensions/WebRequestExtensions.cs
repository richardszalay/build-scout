using System;
using System.Net;
using System.Reactive.Linq;

namespace RichardSzalay.PocketCiTray.Extensions
{
    public static class WebRequestExtensions
    {
        public static IObservable<WebResponse> GetResponseObservable(this WebRequest request)
        {
            return Observable.FromAsyncPattern(
                request.BeginGetResponse,
                result => request.EndGetResponse(result)
                )()
                .Finally(request.Abort);
        }
    }
}