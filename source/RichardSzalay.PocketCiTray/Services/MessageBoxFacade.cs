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
    public interface IMessageBoxFacade
    {
        MessageBoxResult Show(string message, string description, MessageBoxButton buttons);
    }

    public class MessageBoxFacade : IMessageBoxFacade
    {
        public MessageBoxResult Show(string message, string description, MessageBoxButton buttons)
        {
            return MessageBox.Show(message, description, buttons);
        }
    }
}
