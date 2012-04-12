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
    public class CommandLink
    {
        public string Title { get; private set; }
        public ICommand Command { get; private set; }

        public CommandLink(string title, ICommand command)
        {
            this.Title = title;
            this.Command = command;
        }
    }
}
