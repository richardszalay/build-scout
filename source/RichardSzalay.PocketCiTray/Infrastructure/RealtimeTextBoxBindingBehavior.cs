using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class RealtimeTextBoxBindingBehavior : Behavior<TextBox>
    {
        private SerialDisposable disposable = new SerialDisposable();

        protected override void OnAttached()
        {
            disposable.Disposable = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                h => new TextChangedEventHandler(h),
                h => AssociatedObject.TextChanged += h,
                h => AssociatedObject.TextChanged -= h
                )
                .Subscribe(OnEnterPressed);
        }

        protected override void OnDetaching()
        {
            disposable.Dispose();
        }

        private void OnEnterPressed(EventPattern<TextChangedEventArgs> ev)
        {
            var expr = AssociatedObject.GetBindingExpression(TextBox.TextProperty);

            expr.UpdateSource();
        }
    }
}
