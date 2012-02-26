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
using Microsoft.Phone.Shell;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IShellToastFacade
    {
        void Show(string title, string content, Uri navigationUri);
    }

    public class ShellToastFacade : IShellToastFacade
    {
        public void Show(string title, string content, Uri navigationUri)
        {
            new ShellToast
            {
                Title = title,
                Content = content,
                NavigationUri = navigationUri
            }.Show();
        }
    }
}
