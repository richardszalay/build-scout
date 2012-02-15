using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class BindSelectedItemsBehavior : Behavior<MultiselectList>
    {
        public static DependencyProperty BoundListProperty = DependencyProperty.Register("BoundList",
            typeof(IList), typeof(BindSelectedItemsBehavior), new PropertyMetadata(null, PropertyChangedCallback));

        private SerialDisposable disposable = new SerialDisposable();
        private IList selectedItemsList;

        protected override void OnAttached()
        {
            disposable.Disposable = Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                h => new SelectionChangedEventHandler(h),
                h => AssociatedObject.SelectionChanged += h,
                h => AssociatedObject.SelectionChanged -= h
                )
                .Subscribe(OnSelectionChanged);
        }

        protected override void OnDetaching()
        {
            disposable.Dispose();
        }

        private void OnSelectionChanged(EventPattern<SelectionChangedEventArgs> ev)
        {
            if (selectedItemsList == null)
                return;

            foreach (Job job in ev.EventArgs.AddedItems)
            {
                selectedItemsList.Add(job);
            }

            foreach (Job job in ev.EventArgs.RemovedItems)
            {
                selectedItemsList.Remove(job);
            }
        }

        private void SetCollection(IList selectedItemsList)
        {
            this.selectedItemsList = selectedItemsList;

            foreach(var item in AssociatedObject.SelectedItems)
            {
                selectedItemsList.Add(item);
            }
        }

        public IList BoundList
        {
            get { return (IList)GetValue(BoundListProperty); }
            set { SetValue(BoundListProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (BindSelectedItemsBehavior) d;

            behavior.SetCollection((IList)e.NewValue);
        }
    }
}
