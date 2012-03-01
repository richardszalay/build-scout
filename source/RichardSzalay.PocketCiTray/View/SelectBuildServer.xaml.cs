using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.View
{
    public partial class SelectBuildServer
    {
        public SelectBuildServer()
        {
            InitializeComponent();

            ((ContinuumTransition)Resources["ContinuumOutTransition"]).ContinuumElement = BuildServerList;
            ((ContinuumTransition)Resources["ContinuumInTransition"]).ContinuumElement = BuildServerList;
        }
    }
}