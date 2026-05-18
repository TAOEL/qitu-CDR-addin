using System;
using System.Windows;

namespace QiTuCDR
{
    /// <summary>
    /// QiTuCDR 插件全局资源字典。
    /// 作为 CDR VSTA AddIn 加载，资源由宿主进程统一管理。
    /// </summary>
    public partial class AppResources : ResourceDictionary
    {
        /// <summary>
        /// 初始化全局异常处理与日志服务。
        /// 由插件加载入口调用。
        /// </summary>
        public static void Initialize()
        {
            // 初始化全局异常处理
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            }

            // TODO: 后续迭代中初始化日志服务、配置加载等
        }

        /// <summary>
        /// 捕获未处理的 AppDomain 异常，记录日志后避免进程崩溃。
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"[QiTuCDR] 未处理异常: {ex?.Message}");
        }

        /// <summary>
        /// 捕获未处理的 UI 线程异常，阻止应用崩溃。
        /// </summary>
        private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[QiTuCDR] UI线程异常: {e.Exception.Message}");
            e.Handled = true;
        }
    }
}
