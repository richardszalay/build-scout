// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueExpiry.cs" company="XamlNinja">
//   2011 Richard Griffin and Ollie Riches
// </copyright>
// <summary>
// </summary>
// <credits>
// StuartHarris http://red-badger.com/Blog/post/Watching-Silverlight-Dependency-Properties-with-Reactive-Extensions.aspx#comment
// </credits>
// <Updates>
// Due to the introduction of the newly supported ICommand which is now aligned with WPF it means that we can use ICommand directly
// </Updates>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Phone.Shell;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class ApplicationBarMenuItemCommand : ApplicationBarCommand
    {
        private IApplicationBarMenuItem applicationBarMenuItem;

        protected override void Bind()
        {
            base.Bind();

            this.Subscriptions.Add(
                Observable.FromEventPattern(
                    (EventHandler<EventArgs> handler) => new EventHandler(handler),
                    handler => this.applicationBarMenuItem.Click += handler,
                    handler => this.applicationBarMenuItem.Click -= handler).Subscribe(this.OnNextClick));
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.applicationBarMenuItem = this.AssociatedObject.ApplicationBar
                .MenuItems.OfType<ApplicationBarMenuItem>()
                .First(x => x.Text == this.TextKey);

            TextChanged();
            UpdateCanExecute();
        }

        protected override void TextChanged()
        {
            if (this.applicationBarMenuItem != null)
            {
                this.applicationBarMenuItem.Text = this.Text;
            }
        }

        protected override void UpdateCanExecute()
        {
            if (this.applicationBarMenuItem != null && Command != null)
            {
                this.applicationBarMenuItem.IsEnabled = this.Command.CanExecute(this.CommandParameter);
            }
        }
    }
}