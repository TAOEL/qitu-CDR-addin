using System;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// CDR 版本适配策略接口。
    /// 不同 CDR 版本的 API 存在差异，每个版本实现各自的适配策略。
    /// 当前开发测试仅针对 V26（CorelDRAW 2024）。
    /// </summary>
    public interface ICdrVersionStrategy
    {
        /// <summary>
        /// 版本号标识（如 "26.0"）。
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 版本显示名（如 "CorelDRAW 2024"）。
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 获取当前 CDR 应用程序实例。
        /// </summary>
        object GetApplication();

        /// <summary>
        /// 获取当前活动文档。
        /// </summary>
        object GetActiveDocument();

        /// <summary>
        /// 获取当前选中的对象集合。
        /// </summary>
        object GetSelection();

        /// <summary>
        /// 将插件窗口绑定到 CDR 宿主窗口。
        /// </summary>
        /// <param name="pluginWindowHandle">插件窗口句柄</param>
        void AttachToHost(IntPtr pluginWindowHandle);
    }
}
