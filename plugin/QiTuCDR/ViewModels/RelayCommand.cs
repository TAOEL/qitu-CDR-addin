using System;
using System.Windows.Input;

namespace QiTuCDR.ViewModels
{
    /// <summary>
    /// 通用 ICommand 实现。不依赖 CommandManager.RequerySuggested（CDR wpfhost
    /// 中 Application.Current 为 null，CommandManager 静态事件不可靠）。
    /// 改为暴露公共 RaiseCanExecuteChanged() 由 ViewModel 手动触发。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 由 ViewModel 在 CanExecute 条件变更时手动调用，通知 UI 刷新按钮状态。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// 无参数版 RelayCommand（泛型约束简化调用）。
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter is T t ? t : default);
        }

        public void Execute(object parameter)
        {
            if (parameter is T t)
                _execute(t);
        }
    }
}
