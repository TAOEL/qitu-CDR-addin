using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QiTuCDR.ViewModels
{
    /// <summary>
    /// MVVM ViewModel 基类。
    /// 提供 INotifyPropertyChanged 的标准实现，所有 ViewModel 均继承此类。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知 UI 指定属性已变更。
        /// </summary>
        /// <param name="propertyName">属性名（由 CallerMemberName 自动填充）</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并在变更时自动通知 UI。
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名（自动填充）</param>
        /// <returns>值是否发生变化</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
