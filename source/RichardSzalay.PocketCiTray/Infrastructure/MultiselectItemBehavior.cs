using System;
using System.Linq;
using System.Collections;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;
using LinqToVisualTree;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class MultiselectItemBehavior : Behavior<FrameworkElement>
    {
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
            var list = (MultiselectList)AssociatedObject.Ancestors<MultiselectList>().FirstOrDefault();

            if (list != null)
            {
                var item = AssociatedObject.DataContext;

                MultiselectItem container = list.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
        }
    }
}
