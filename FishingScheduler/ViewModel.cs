using System.ComponentModel;

namespace FishingScheduler
{
    public abstract class ViewModel
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void UpdateProperty<T>(ref T OldValue, T NewValue, string PropertyName)
        {
            if (!OldValue.Equals(NewValue))
            {
                OldValue = NewValue;
                RaisePropertyChangedEvent(PropertyName);
            }
        }

        protected void RaisePropertyChangedEvent(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
