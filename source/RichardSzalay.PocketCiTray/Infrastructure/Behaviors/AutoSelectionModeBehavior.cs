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
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class AutoSelectionModeBehavior : Behavior<MultiselectList>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssociatedObject.IsSelectionEnabled = AssociatedObject.SelectedItems.Count > 0;
        }
    }
}
