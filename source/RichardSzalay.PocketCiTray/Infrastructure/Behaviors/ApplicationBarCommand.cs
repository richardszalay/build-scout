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
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public abstract class ApplicationBarCommand : Behavior<PhoneApplicationPage>
    {
        public static readonly DependencyProperty CommandBindingProperty = DependencyProperty.Register(
            "CommandBinding",
            typeof(ICommand),
            typeof(ApplicationBarCommand),
            new PropertyMetadata(CommandBindingChanged));

        public static readonly DependencyProperty CommandParameterBindingProperty =
            DependencyProperty.Register(
                "CommandParameterBinding",
                typeof(object),
                typeof(ApplicationBarCommand),
                new PropertyMetadata(CommandParameterBindingChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(String), typeof(ApplicationBarCommand), new PropertyMetadata(TextChanged));

        private ICommand command;

        private object commandParameter;

        private CompositeDisposable subscriptions;

        public ICommand Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (value != this.command)
                {
                    this.command = value;
                    this.CommandChanged();
                }
            }
        }

        public ICommand CommandBinding
        {
            get
            {
                return (ICommand)this.GetValue(CommandBindingProperty);
            }

            set
            {
                this.SetValue(CommandBindingProperty, value);
            }
        }

        public object CommandParameter
        {
            get
            {
                return this.commandParameter;
            }

            set
            {
                if (value != this.commandParameter)
                {
                    this.commandParameter = value;
                    this.CommandParameterChanged();
                }
            }
        }

        public object CommandParameterBinding
        {
            get
            {
                return (object)this.GetValue(CommandParameterBindingProperty);
            }

            set
            {
                this.SetValue(CommandParameterBindingProperty, value);
            }
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }

        public string TextKey { get; set; }

        protected CompositeDisposable Subscriptions
        {
            get
            {
                return this.subscriptions;
            }
        }

        protected virtual void Bind()
        {
            this.subscriptions = new CompositeDisposable();
            this.BindCommand();
            //this.BindCommandParameter();
            this.UpdateCanExecute();
            //this.BindText();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            Observable.FromEventPattern(
                (EventHandler<RoutedEventArgs> h) => new RoutedEventHandler(h),
                handler => this.AssociatedObject.Loaded += handler,
                handler => this.AssociatedObject.Loaded -= handler).Subscribe(@event => this.Bind());
            Observable.FromEventPattern(
                (EventHandler<RoutedEventArgs> h) => new RoutedEventHandler(h),
                handler => this.AssociatedObject.Unloaded += handler,
                handler => this.AssociatedObject.Unloaded -= handler).Subscribe(@event => this.UnBind());
        }

        protected void OnNextClick(EventPattern<EventArgs> @event)
        {
            if (this.CommandBinding != null)
            {
                this.CommandBinding.Execute(this.commandParameter);
            }
        }

        protected static void TextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ((ApplicationBarCommand)dependencyObject).TextChanged();
        }

        protected abstract void TextChanged();

        protected abstract void UpdateCanExecute();

        private static void CommandBindingChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var @this = dependencyObject as ApplicationBarCommand;
            if (@this != null)
            {
                @this.command = (ICommand)args.NewValue;
                @this.CommandBindingChanged();
            }
        }

        private static void CommandParameterBindingChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var @this = dependencyObject as ApplicationBarCommand;
            if (@this != null)
            {
                @this.commandParameter = (object) args.NewValue;
                @this.CommandParameterBindingChanged();
            }
        }

        private static void TextBindingChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var @this = dependencyObject as ApplicationBarCommand;
            if (@this != null)
            {
                @this.TextBindingChanged();
            }
        }

        private void BindCommand()
        {
            if (command != null)
            {
                this.CommandChanged();
            }
        }

        /*
        private void BindCommandParameter()
        {
            Binding binding = this.CommandParameterBinding;
            if (binding != null)
            {
                this.subscriptions.Add(
                    this.AssociatedObject.GetChanges(binding).Subscribe(
                        args =>
                        {
                            this.commandParameter = args.NewValue;
                            this.CommandParameterChanged();
                        }));
            }
        }
        

        private void BindText()
        {
            Binding binding = this.TextBinding;
            if (binding != null)
            {
                this.subscriptions.Add(
                    this.AssociatedObject.GetChanges(binding).Subscribe(
                        args =>
                        {
                            this.Text = args.NewValue as string;
                            this.TextChanged();
                        }));

                this.BindCommand();
            }
        } */

        private void CommandBindingChanged()
        {
            if (this.AssociatedObject != null)
            {
                this.BindCommand();
            }
        }

        private void CommandChanged()
        {
            if (this.command != null && subscriptions != null)
            {
                this.Subscriptions.Add(
                Observable.FromEventPattern(
                      (EventHandler<EventArgs> handler) => new EventHandler(handler),
                        handler => this.command.CanExecuteChanged += handler,
                        handler => this.command.CanExecuteChanged -= handler).Subscribe(this.OnNextCanExecuteChanged));

                this.UpdateCanExecute();
            }
        }

        private void CommandParameterBindingChanged()
        {
            if (this.AssociatedObject != null)
            {
                this.UpdateCanExecute();
                //this.BindCommandParameter();
            }
        }

        private void CommandParameterChanged()
        {
            this.UpdateCanExecute();
        }

        private void OnNextCanExecuteChanged(EventPattern<EventArgs> @event)
        {
            this.UpdateCanExecute();
        }

        private void TextBindingChanged()
        {
            if (this.AssociatedObject != null)
            {
            }
        }

        private void UnBind()
        {
            if (this.subscriptions != null)
            {
                this.subscriptions.Dispose();
            }
        }
    }
}