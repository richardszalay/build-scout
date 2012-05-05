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
using System.Windows;
using System.Windows.Data;
using Microsoft.Phone.Shell;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class ApplicationBarIconButtonCommand : ApplicationBarCommand
    {
        public static readonly DependencyProperty IconBindingProperty = DependencyProperty.Register(
            "IconBinding",
            typeof(Binding),
            typeof(ApplicationBarIconButtonCommand),
            new PropertyMetadata(IconBindingChanged));

        private IApplicationBarIconButton applicationBarIconButton;

        private Uri icon;

        public Uri Icon
        {
            get
            {
                return this.icon;
            }

            set
            {
                if (value != this.icon)
                {
                    this.icon = value;
                    this.IconChanged();
                }
            }
        }

        public Binding IconBinding
        {
            get
            {
                return (Binding)this.GetValue(IconBindingProperty);
            }

            set
            {
                this.SetValue(IconBindingProperty, value);
            }
        }

        protected override void Bind()
        {
            base.Bind();

            this.Subscriptions.Add(
               Observable.FromEventPattern(
                   (EventHandler<EventArgs> handler) => new EventHandler(handler),
                   handler => this.applicationBarIconButton.Click += handler,
                   handler => this.applicationBarIconButton.Click -= handler).Subscribe(this.OnNextClick));
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.applicationBarIconButton = this.AssociatedObject.ApplicationBar
                .Buttons.OfType<ApplicationBarIconButton>()
                .First(x => x.Text == this.TextKey);

            TextChanged();
            UpdateCanExecute();
            IconBindingChanged();
        }

        protected override void TextChanged()
        {
            if (this.applicationBarIconButton != null && Text != null)
            {
                this.applicationBarIconButton.Text = this.Text;
            }
        }

        protected override void UpdateCanExecute()
        {
            if (this.applicationBarIconButton != null && Command != null)
            {
                this.applicationBarIconButton.IsEnabled = this.Command.CanExecute(this.CommandParameter);
            }
        }

        private static void IconBindingChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var @this = dependencyObject as ApplicationBarIconButtonCommand;
            if (@this != null)
            {
                @this.IconBindingChanged();
            }
        }

        private void IconBindingChanged()
        {
            if (this.AssociatedObject != null)
            {
                //this.BindIcon();
            }
        }

        private void IconChanged()
        {
            if (this.applicationBarIconButton != null && this.icon != null)
            {
                this.applicationBarIconButton.IconUri = this.icon;
            }
        }
    }
}