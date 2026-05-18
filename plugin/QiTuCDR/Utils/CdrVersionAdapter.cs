using System;
using System.Collections.Generic;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// CDR 版本适配器（策略模式 Context）。
    /// 根据当前运行的 CDR 版本自动选择对应的适配策略。
    ///
    /// 开发规则：当前仅实现并测试 V26（CorelDRAW 2024），
    /// V27（CorelDRAW 2025）为禁用目标——不实现、不使用、不测试。
    /// 后续版本兼容性验证时再逐步添加其他策略。
    /// </summary>
    public class CdrVersionAdapter
    {
        /// <summary>
        /// 已注册的版本策略字典。
        /// </summary>
        private readonly Dictionary<string, ICdrVersionStrategy> _strategies;

        /// <summary>
        /// 当前激活的策略。
        /// </summary>
        private ICdrVersionStrategy _currentStrategy;

        /// <summary>
        /// 单例实例。
        /// </summary>
        private static readonly Lazy<CdrVersionAdapter> _instance =
            new Lazy<CdrVersionAdapter>(() => new CdrVersionAdapter());

        /// <summary>
        /// 获取全局单例。
        /// </summary>
        public static CdrVersionAdapter Instance => _instance.Value;

        private CdrVersionAdapter()
        {
            _strategies = new Dictionary<string, ICdrVersionStrategy>();

            // 注册已知版本的策略
            RegisterStrategy(new CdrV26Strategy());

            // 默认使用 V26
            _currentStrategy = _strategies.ContainsKey("26.0")
                ? _strategies["26.0"]
                : null;
        }

        /// <summary>
        /// 注册一个版本策略。
        /// </summary>
        public void RegisterStrategy(ICdrVersionStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _strategies[strategy.Version] = strategy;
        }

        /// <summary>
        /// 根据 CDR 版本号切换策略。
        /// </summary>
        /// <param name="version">版本号（如 "26.0"）</param>
        /// <returns>是否切换成功</returns>
        public bool SwitchToVersion(string version)
        {
            if (_strategies.TryGetValue(version, out var strategy))
            {
                _currentStrategy = strategy;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 当前激活的 CDR 版本号。
        /// </summary>
        public string CurrentVersion => _currentStrategy?.Version ?? "未知";

        /// <summary>
        /// 当前 CDR 版本显示名。
        /// </summary>
        public string CurrentDisplayName => _currentStrategy?.DisplayName ?? "未知版本";

        /// <summary>
        /// 获取 CDR 应用程序实例。
        /// </summary>
        public object GetApplication()
        {
            EnsureStrategy();
            return _currentStrategy?.GetApplication();
        }

        /// <summary>
        /// 获取当前活动文档。
        /// </summary>
        public object GetActiveDocument()
        {
            EnsureStrategy();
            return _currentStrategy?.GetActiveDocument();
        }

        /// <summary>
        /// 获取当前选中对象。
        /// </summary>
        public object GetSelection()
        {
            EnsureStrategy();
            return _currentStrategy?.GetSelection();
        }

        /// <summary>
        /// 将插件窗口绑定到 CDR 宿主窗口。
        /// </summary>
        public void AttachToHost(IntPtr pluginWindowHandle)
        {
            EnsureStrategy();
            _currentStrategy?.AttachToHost(pluginWindowHandle);
        }

        /// <summary>
        /// 确认当前策略可用，否则回退到默认。
        /// </summary>
        private void EnsureStrategy()
        {
            if (_currentStrategy == null && _strategies.Count > 0)
            {
                // 回退到第一个注册的策略
                foreach (var kv in _strategies)
                {
                    _currentStrategy = kv.Value;
                    break;
                }
            }
        }
    }
}
