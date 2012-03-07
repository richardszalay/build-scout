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

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class PageLink
    {
        public PageLink(string title, string description, Uri navigateUri)
        {
            this.Title = title;
            this.Description = description;
            this.NavigateUri = navigateUri;
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public Uri NavigateUri { get; private set; }
    }
}
