using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Extensions;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public abstract class ViewModelBase : PropertyChangeBase
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        protected CompositeDisposable Disposables { get { return disposables; } }

        /// <summary>
        /// Convenience method for subscribing to an ObservableCommand and then assigning ICommand to a local field. 
        /// Subscription will be terminated when the user navigates away from the page.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected ICommand CreateCommand(ObservableCommand command, Action action)
        {
            disposables.Add(command.Subscribe(_ => action()));

            return command;
        }

        public virtual void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public virtual void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        protected ICommand CreateCommand<TParam>(ObservableCommand<TParam> command, Action<TParam> action)
        {
            disposables.Add(command.Subscribe(action));

            return command;
        }
    }
}