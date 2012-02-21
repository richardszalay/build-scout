using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Service;
using Microsoft.Silverlight.Testing.Harness;

namespace RichardSzalay.PocketCiTray.Tests
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UnitTestSettings settings = new UnitTestSettings();
            settings.TestAssemblies.Add(this.GetType().Assembly);
            settings.TestService = new WindowsPhoneTestService(settings);

            UnitTestSystem.SetStandardLogProviders(settings);

            settings.TestHarness = new UnitTestHarness();

            var testPage = UnitTestSystem.CreateTestPage() as IMobileTestPage;

            BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
            (Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;
        }
    }
}