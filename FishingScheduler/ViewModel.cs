using System;
using System.ComponentModel;

namespace FishingScheduler
{
    public abstract class ViewModel
        : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;
        public event PropertyChangedEventHandler PropertyChanged;


        protected ViewModel()
        {

        }

        protected void RaisePropertyChangedEvent(string PropertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

            }
            catch (Exception)
            {
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // マネージドリソースの解放
                }

                // アンマネージドリソースの解放
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
