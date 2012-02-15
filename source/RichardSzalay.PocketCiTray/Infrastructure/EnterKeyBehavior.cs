using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class EnterKeyBehavior : Behavior<TextBox>
    {
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof(ICommand), typeof(EnterKeyBehavior), new PropertyMetadata(null));

        private SerialDisposable disposable = new SerialDisposable();

        protected override void OnAttached()
        {
            disposable.Disposable = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                h => new KeyEventHandler(h),
                h => AssociatedObject.KeyDown += h,
                h => AssociatedObject.KeyDown -= h
                )
                .Where(e => e.EventArgs.Key == Key.Enter)
                .Subscribe(OnEnterPressed);
        }

        protected override void OnDetaching()
        {
            disposable.Dispose();
        }

        private void OnEnterPressed(EventPattern<KeyEventArgs> ev)
        {
            string text = AssociatedObject.Text;

            if (Command != null && Command.CanExecute(text))
            {
                Command.Execute(text);
            }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
