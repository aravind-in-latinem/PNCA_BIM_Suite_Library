using System;
using System.Windows.Input;

namespace PNCA_BIM_Suite_Library.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #region Normal Version of CanExecute
        //public bool CanExecute(object parameter)
        //{
        //    if (_canExecute != null)
        //    {
        //        return _canExecute.Invoke();
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        #endregion


        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}