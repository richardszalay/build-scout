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

namespace RichardSzalay.PocketCiTray
{
    [Flags]
    public enum BuildResultChange
    {
        None = 1,
        Fixed = 2,
        Failed = 4,
        Unavailable = 8
    }
}
