using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.View
{
    public partial class ViewJob
    {
        public ViewJob()
        {
            InitializeComponent();

            ((ContinuumTransition)Resources["ContinuumInTransition"]).ContinuumElement = ApplicationTitle;
            ((ContinuumTransition)Resources["ContinuumOutTransition"]).ContinuumElement = ApplicationTitle;
        }
    }
}