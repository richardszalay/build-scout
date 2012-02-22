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

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class ListBoxSelectionCommandBehavior : Behavior<ListBox>
    {   
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ListBoxSelectionCommandBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty ResetSelectedItemProperty =
            DependencyProperty.Register("ResetSelectedItem", typeof(bool), typeof(ListBoxSelectionCommandBehavior), new PropertyMetadata(false));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool ResetSelectedItem
        {
            get { return (bool)GetValue(ResetSelectedItemProperty); }
            set { SetValue(ResetSelectedItemProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var commandParameter = e.AddedItems[0];

                if (Command != null && Command.CanExecute(commandParameter))
                {
                    Command.Execute(commandParameter);
                }

                if (ResetSelectedItem)
                {
                    AssociatedObject.SelectedIndex = -1;
                }
            }
        }


    }
}
