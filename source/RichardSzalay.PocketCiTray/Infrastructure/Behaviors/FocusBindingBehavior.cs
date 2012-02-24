using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Diagnostics;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class FocusBindingBehavior : Behavior<Control>
    {
        public bool IsFocussed
        {
            get { return (bool)GetValue(IsFocussedProperty); }
            set { SetValue(IsFocussedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFocussedProperty =
            DependencyProperty.Register("IsFocussed", typeof(bool), typeof(FocusBindingBehavior), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (FocusBindingBehavior)d;

            if (behavior.AssociatedObject != null)
            {
                bool focus = (bool)e.NewValue;

                if (focus)
                {
                    behavior.AssociatedObject.Focus();
                }
            }
        }
    }
}
