using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.Common
{
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;
        private bool isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;
            isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try { await execute(); }
            finally
            {
                isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
