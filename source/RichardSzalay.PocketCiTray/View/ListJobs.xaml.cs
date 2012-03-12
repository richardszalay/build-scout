using RichardSzalay.PocketCiTray.Infrastructure;
using WP7Contrib.View.Transitions.Animation;
using System;
using System.Diagnostics;
namespace RichardSzalay.PocketCiTray.View
{
    public partial class ListJobs
    {
        public ListJobs()
        {
            Debug.WriteLine("[{0:hh:mm:ss.fff}] Begin ListJobs.ctor", DateTimeOffset.UtcNow);

            InitializeComponent();

            ((ContinuumTransition) Resources["ContinuumOutTransition"]).ContinuumElement = JobsList;
            ((ContinuumTransition)Resources["ContinuumInTransition"]).ContinuumElement = JobsList;

            Debug.WriteLine("[{0:hh:mm:ss.fff}] End ListJobs.ctor", DateTimeOffset.UtcNow);
        }

        /*
        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (toOrFrom != null && toOrFrom.OriginalString.Contains("ViewJob"))
            {
                return GetContinuumAnimation(JobsList, animationType);
            }

            if (animationType == AnimationType.NavigateForwardIn ||
                animationType == AnimationType.NavigateBackwardIn)
            {
                return new TurnstileFeatherForwardInAnimator
                {
                    ListBox = JobsList,
                    RootElement = AnimationContext
                };
            }
            return base.GetAnimation(animationType, toOrFrom);
        }*/
    }
}