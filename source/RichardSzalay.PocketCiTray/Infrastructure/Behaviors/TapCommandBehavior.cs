﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class TapCommandBehavior : Behavior<FrameworkElement>
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }



        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(TapCommandBehavior), new PropertyMetadata(null));



        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TapCommandBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.Tap += OnTap; 
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Tap -= OnTap;
        }

        private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
