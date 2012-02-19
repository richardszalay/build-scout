using WP7Contrib.View.Transitions.Animation;
using System;
namespace RichardSzalay.PocketCiTray.View
{
    public partial class ListJobs
    {
        public ListJobs()
        {
            InitializeComponent();

            //AnimationContext = LayoutRoot;
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