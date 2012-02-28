using System;
using System.Collections;
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class RecurringDaysPickerNoneSelectedTextBehavior : Behavior<RecurringDaysPicker>
    {
        public string NoneSelectedText
        {
            get { return (string)GetValue(NoneSelectedTextProperty); }
            set { SetValue(NoneSelectedTextProperty, value); }
        }

        public static readonly DependencyProperty NoneSelectedTextProperty =
            DependencyProperty.Register("NoneSelectedText", typeof(string), typeof(RecurringDaysPickerNoneSelectedTextBehavior), new PropertyMetadata(null));

        

        private Func<IList, string> originalDelegate;

        protected override void OnAttached()
        {
            base.OnAttached();

            originalDelegate = AssociatedObject.SummaryForSelectedItemsDelegate;

            AssociatedObject.SummaryForSelectedItemsDelegate = GetSelectedItemsText;
        }

        private string GetSelectedItemsText(IList arg)
        {
            if (arg == null || arg.Count == 0)
            {
                return NoneSelectedText;
            }

            return originalDelegate(arg);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SummaryForSelectedItemsDelegate = originalDelegate;
        }
    }
}
