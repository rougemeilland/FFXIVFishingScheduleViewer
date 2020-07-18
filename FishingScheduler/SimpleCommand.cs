using System;
using System.Windows.Input;

namespace FishingScheduler
{
    class SimpleCommand
        : ICommand
    {
        private Action<object> _action;

        public SimpleCommand(Action<object> action)
        {
            _action = action;
        }

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning disable 0067

        public bool CanExecute(object parameter)
        {
            return _action != null;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
