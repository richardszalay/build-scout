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
    public class EmailComposeTaskFacade : IEmailComposeTaskFacade
    {
        public void Show(string to, string subject)
        {
            new EmailComposeTask
            {
                To = to,
                Subject = subject
            }.Show();
        }

    }

    public interface IEmailComposeTaskFacade
    {
        void Show(string to, string subject);
    }
}
