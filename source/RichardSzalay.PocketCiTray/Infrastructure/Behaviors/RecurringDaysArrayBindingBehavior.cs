using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Interactivity;

namespace RichardSzalay.PocketCiTray.Infrastructure.Behaviors
{
    public class RecurringDaysArrayBindingBehavior : Behavior<RecurringDaysPicker>
    {
        public DayOfWeek[] SelectedItems
        {
            get { return (DayOfWeek[])GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(DayOfWeek[]), typeof(RecurringDaysArrayBindingBehavior), new PropertyMetadata(OnSourceBindingChanged));

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;

            base.OnAttached();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = AssociatedObject.SelectedItems.OfType<DayOfWeek>().ToArray();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void UpdateTargetBindng()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItems.Clear();

                foreach (var dayOfWeek in SelectedItems)
                {
                    AssociatedObject.SelectedItems.Add(dayOfWeek);
                }
            }
        }

        private static void OnSourceBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RecurringDaysArrayBindingBehavior)sender;

            behavior.UpdateTargetBindng();            
        }
    }
}
