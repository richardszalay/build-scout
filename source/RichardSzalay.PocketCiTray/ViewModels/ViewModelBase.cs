using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using RichardSzalay.PocketCiTray.Extensions.Extensions;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public abstract class ViewModelBase : PropertyChangeBase
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        private ProgressIndicator progressIndicator;

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
            this.Disposables.Dispose();
            this.disposables = new CompositeDisposable();
        }

        public virtual void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        protected void StartLoading(string status)
        {
            ProgressIndicator = new ProgressIndicator
            {
                IsIndeterminate = true,
                IsVisible = true,
                Text = status,
            };
        }

        protected void StopLoading()
        {
            ProgressIndicator = new ProgressIndicator
            {
                IsVisible = false
            };
        }

        public ProgressIndicator ProgressIndicator
        {
            get { return progressIndicator; }
            protected set { progressIndicator = value; OnPropertyChanged("ProgressIndicator"); }
        }

        protected ICommand CreateCommand<TParam>(ObservableCommand<TParam> command, Action<TParam> action)
        {
            disposables.Add(command.Subscribe(action));

            return command;
        }

        protected void StartLoading()
        {
            StartLoading("");
        }
    }
}