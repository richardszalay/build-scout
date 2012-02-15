using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace RichardSzalay.PocketCiTray.Extensions
{
    public class ObservableCommand<TParam> : ICommand, IDisposable, IObservable<TParam>
    {
        private readonly BehaviorSubject<bool> canExecuteSubject = new BehaviorSubject<bool>(false);
        private readonly Subject<TParam> values = new Subject<TParam>();

        private readonly IDisposable subscription;

        public ObservableCommand(IObservable<bool> canExecute)
        {
            subscription = canExecute
                .Subscribe(value =>
                {
                    canExecuteSubject.OnNext(value);
                    OnCanExecuteChanged();
                });
        }

        public ObservableCommand()
            : this(Observable.Return(true))
        {
            
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteSubject.First();
        }

        public void Execute(object parameter)
        {
            TParam paramValue = (parameter == null)
                ? default(TParam)
                : (TParam)parameter;

            this.values.OnNext(paramValue);
        }

        protected void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;
        
        public void Dispose()
        {
            subscription.Dispose();
        }

        public IDisposable Subscribe(IObserver<TParam> observer)
        {
            return values.Subscribe(observer);
        }
    }

    public class ObservableCommand : ObservableCommand<Unit>
    {
        public ObservableCommand(IObservable<bool> canExecute) : base(canExecute)
        {
        }

        public ObservableCommand()
            : base()
        {

        }

    }
}