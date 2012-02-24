using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Diagnostics;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class VisualStateBindingBehavior : Behavior<Control>
    {
        public string State
        {
            get { return (string)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(VisualStateBindingBehavior), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (VisualStateBindingBehavior) d;

            if (behavior.AssociatedObject != null)
            {
                bool result = VisualStateManager.GoToState(behavior.AssociatedObject, (string)e.NewValue, true);

                Debug.WriteLine(result);
            }
        }
    }
}
