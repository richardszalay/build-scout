using System.ComponentModel;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public abstract class PropertyChangeBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;
    }
}