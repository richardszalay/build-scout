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
            disposable.Disposable = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                h => new KeyEventHandler(h),
                h => AssociatedObject.KeyDown += h,
                h => AssociatedObject.KeyDown -= h
                )
                .Subscribe(OnEnterPressed);
        }

        protected override void OnDetaching()
        {
            disposable.Dispose();
        }

        private void OnEnterPressed(EventPattern<KeyEventArgs> ev)
        {
            var expr = AssociatedObject.GetBindingExpression(TextBox.TextProperty);

            expr.UpdateSource();
        }
    }
}
