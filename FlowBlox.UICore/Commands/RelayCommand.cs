using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeWithParam;
        private readonly Action _execute;
        private readonly Func<object, bool> _canExecuteWithParam;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteWithParam = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute();

            return _canExecuteWithParam == null || _canExecuteWithParam(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute();
            else
                _executeWithParam(parameter);
        }

        
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Invalidate()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
