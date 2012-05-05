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
    public interface IApplicationMarketplaceFacade
    {
        void ShowDetail();
        void ShowReview();
    }

    public class ApplicationMarketplaceFacade : IApplicationMarketplaceFacade
    {
        private readonly IApplicationInformation applicationInformation;

        public ApplicationMarketplaceFacade(IApplicationInformation applicationInformation)
        {
            this.applicationInformation = applicationInformation;
        }

        public void ShowDetail()
        {
            new MarketplaceDetailTask
            {
                ContentIdentifier = applicationInformation.ProductId
            }.Show();
        }

        public void ShowReview()
        {
            new MarketplaceReviewTask().Show();
        }
    }
}
