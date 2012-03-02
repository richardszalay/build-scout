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
    public class PhoneApplicationServiceFacade : RichardSzalay.PocketCiTray.Services.IPhoneApplicationServiceFacade
    {
        private readonly PhoneApplicationService phoneApplicationService;

        public PhoneApplicationServiceFacade(PhoneApplicationService phoneApplicationService)
        {
            this.phoneApplicationService = phoneApplicationService;
        }

        public IdleDetectionMode ApplicationIdleDetectionMode
        {
            get { return phoneApplicationService.ApplicationIdleDetectionMode; }
            set { phoneApplicationService.ApplicationIdleDetectionMode = value; }
        }
    }
}
