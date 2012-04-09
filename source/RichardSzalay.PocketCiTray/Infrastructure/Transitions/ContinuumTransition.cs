using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using WP7Contrib.View.Controls.Extensions;
using System;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class ContinuumTransition : TransitionElement
    {
        public const string ContinuumElementPropertyName = "ContinuumElement";
        public const string ContinuumModePropertyName = "Mode";

        public event EventHandler<ResolvingContinuumElementEventArgs> ResolvingContinuumElement;

        public FrameworkElement ContinuumElement
        {
            get { return (FrameworkElement)GetValue(ContinuumElementProperty); }
            set { SetValue(ContinuumElementProperty, value); }
        }

        public ContinuumTransitionMode Mode
        {
            get { return (ContinuumTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ContinuumElementProperty =
           DependencyProperty.Register(ContinuumElementPropertyName, typeof(FrameworkElement), typeof(ContinuumTransition), new PropertyMetadata(null));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(ContinuumModePropertyName, typeof(ContinuumTransitionMode), typeof(ContinuumTransition), null);


        public ContinuumTransition() { }
        public ContinuumTransition(ContinuumTransitionMode mode)
        {
            Mode = mode;
        }
        public ContinuumTransition(ContinuumTransitionMode mode, FrameworkElement element)
        {
            Mode = mode;
            ContinuumElement = element;
        }

        public override ITransition GetTransition(UIElement element)
        {
            Storyboard storyboard = null;
            if (Mode == ContinuumTransitionMode.BackwardIn)
                storyboard = XamlReader.Load(ContinuumBackwardInStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.BackwardOut)
                storyboard = XamlReader.Load(ContinuumBackwardOutStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.ForwardIn)
                storyboard = XamlReader.Load(ContinuumForwardInStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.ForwardOut)
                storyboard = XamlReader.Load(ContinuumForwardOutStoryboard) as Storyboard;

            ContinuumElement.GetTransform<CompositeTransform>(TransformCreationMode.CreateOrAddAndIgnoreMatrix);

            SetTargets(new Dictionary<string, FrameworkElement>()
                {
                    { "LayoutRoot", element as FrameworkElement },
                    { ContinuumElementPropertyName, GetActualContuumElement() }
                },
                storyboard);

            return new Transition(element, storyboard);
        }

        private FrameworkElement GetActualContuumElement()
        {
            Selector selector = ContinuumElement as Selector;

            bool needsList = (Mode == ContinuumTransitionMode.ForwardOut || Mode == ContinuumTransitionMode.BackwardIn);
            bool isList = selector != null;

            FrameworkElement element = ContinuumElement;

            if (needsList && isList && selector.SelectedIndex != -1)
            {
                element = (FrameworkElement)selector.ItemContainerGenerator.ContainerFromIndex(selector.SelectedIndex);
            }

            OnResolvingContinuumElement(ref element);

            return element;
        }

        private void OnResolvingContinuumElement(ref FrameworkElement element)
        {
            var args = new ResolvingContinuumElementEventArgs(element);

            var handler = this.ResolvingContinuumElement;

            if (handler != null)
            {
                handler(this, args);

                element = args.ContinuumElement;
            }
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets, Storyboard sb)
        {
            foreach (var kvp in targets)
            {
                kvp.Value.RenderTransform = new CompositeTransform();

                var timelines = sb.Children.Where(t => Storyboard.GetTargetName(t) == kvp.Key);
                foreach (Timeline t in timelines)
                    Storyboard.SetTarget(t, kvp.Value);
            }
        }

        internal static readonly string ContinuumForwardOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""70"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
	        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot"">
		        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
		        <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
			        <EasingDoubleKeyFrame.EasingFunction>
				        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
			        </EasingDoubleKeyFrame.EasingFunction>
		        </EasingDoubleKeyFrame>
	        </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""73"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""225"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
			<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""ContinuumElement"" Storyboard.TargetProperty=""(UIElement.Opacity)"">
				<DiscreteDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"" />
			</DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        internal static readonly string ContinuumForwardInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""50""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-70""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""130""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.15"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        internal static readonly string ContinuumBackwardOutStoryboard =
        @"<Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""50"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.15"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        internal static readonly string ContinuumBackwardInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-70""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-30""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
			<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""ContinuumElement"" Storyboard.TargetProperty=""(UIElement.Opacity)"">
				<DiscreteDoubleKeyFrame KeyTime=""0:0:0"" Value=""1"" />
			</DoubleAnimationUsingKeyFrames>
	        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot"">
		        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
		        <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""1"">
			        <EasingDoubleKeyFrame.EasingFunction>
				        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
			        </EasingDoubleKeyFrame.EasingFunction>
		        </EasingDoubleKeyFrame>
	        </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Duration=""0"" To=""0"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot""/>
        </Storyboard>";
    }

    public enum ContinuumTransitionMode
    {
        ForwardOut,
        ForwardIn,
        BackwardOut,
        BackwardIn
    }

    public class ResolvingContinuumElementEventArgs : EventArgs
    {
        public FrameworkElement ContinuumElement { get; set; }

        public ResolvingContinuumElementEventArgs(FrameworkElement continuumElement)
        {
            this.ContinuumElement = continuumElement;
        }
    }
}
