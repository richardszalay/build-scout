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
using Microsoft.Phone.Tasks;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IWebBrowserTaskFacade
    {
        void Show(Uri uri);
    }

    public class WebBrowserTaskFacade : IWebBrowserTaskFacade
    {
        public void Show(Uri uri)
        {
            new WebBrowserTask()
            {
                Uri = uri
            }.Show();
        }
    }
}
