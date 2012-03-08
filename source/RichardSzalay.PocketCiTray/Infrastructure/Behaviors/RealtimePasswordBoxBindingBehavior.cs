using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class RealtimePasswordBoxBindingBehavior : Behavior<PasswordBox>
    {
        private SerialDisposable disposable = new SerialDisposable();

        protected override void OnAttached()
        {
            disposable.Disposable = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => new RoutedEventHandler(h),
                h => AssociatedObject.PasswordChanged += h,
                h => AssociatedObject.PasswordChanged -= h
                )
                .Subscribe(OnEnterPressed);
        }

        protected override void OnDetaching()
        {
            disposable.Dispose();
        }

        private void OnEnterPressed(EventPattern<RoutedEventArgs> ev)
        {
            var expr = AssociatedObject.GetBindingExpression(PasswordBox.PasswordProperty);

            expr.UpdateSource();
        }
    }
}
