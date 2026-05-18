using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QiTuCDR.Utils
{
    internal static class PluginSettings
    {
        private static bool _loaded;
        private static bool _isTopmost = true;
        private static bool _isPositionMemory = true;
        private static bool _isSaveConfig = true;
        private static readonly Dictionary<string, (double Left, double Top)> _positions = new Dictionary<string, (double Left, double Top)>();

        private static readonly string SettingsFilePath;

        static PluginSettings()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(appData, "QiTuCDR");
            Directory.CreateDirectory(dir);
            SettingsFilePath = Path.Combine(dir, "settings.json");
        }

        /// <summary>
        /// 窗口置顶开关。设为 false 时解除主窗口与 CDR 的父子绑定，
        /// 设为 true 时通过 GWLP_HWNDPARENT 绑定到 CDR 主窗口（仅 CDR 内部置顶）。
        /// </summary>
        public static bool IsTopmost
        {
            get { EnsureLoaded(); return _isTopmost; }
            set { _isTopmost = value; TrySave(); }
        }

        /// <summary>
        /// 窗口位置记忆开关。关闭后清除已保存的位置，每次打开默认屏幕居中。
        /// </summary>
        public static bool IsPositionMemory
        {
            get { EnsureLoaded(); return _isPositionMemory; }
            set
            {
                _isPositionMemory = value;
                if (!value)
                    _positions.Clear();
                TrySave();
            }
        }

        /// <summary>
        /// 保存配置参数开关。开启时将所有设置持久化到磁盘，关闭时仅内存生效。
        /// </summary>
        public static bool IsSaveConfig
        {
            get { EnsureLoaded(); return _isSaveConfig; }
            set { _isSaveConfig = value; TrySave(); }
        }

        /// <summary>
        /// 保存窗口位置（仅在 IsPositionMemory 开启时调用）。
        /// </summary>
        public static void SavePosition(string key, double left, double top)
        {
            _positions[key] = (left, top);
        }

        /// <summary>
        /// 尝试获取已保存的窗口位置。
        /// </summary>
        public static bool TryGetPosition(string key, out double left, out double top)
        {
            if (_positions.TryGetValue(key, out var pos))
            {
                left = pos.Left;
                top = pos.Top;
                return true;
            }
            left = 0;
            top = 0;
            return false;
        }

        /// <summary>
        /// 主动持久化所有设置到磁盘。
        /// </summary>
        public static void Save()
        {
            try
            {
                var data = new SettingsData
                {
                    IsTopmost = _isTopmost,
                    IsPositionMemory = _isPositionMemory,
                    IsSaveConfig = _isSaveConfig,
                    Positions = new Dictionary<string, double[]>()
                };

                foreach (var kv in _positions)
                {
                    data.Positions[kv.Key] = new[] { kv.Value.Left, kv.Value.Top };
                }

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch { }
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;

            try
            {
                if (!File.Exists(SettingsFilePath)) return;

                var json = File.ReadAllText(SettingsFilePath);
                var data = JsonConvert.DeserializeObject<SettingsData>(json);
                if (data == null) return;

                _isTopmost = data.IsTopmost;
                _isPositionMemory = data.IsPositionMemory;
                _isSaveConfig = data.IsSaveConfig;

                if (data.Positions != null)
                {
                    foreach (var kv in data.Positions)
                    {
                        if (kv.Value != null && kv.Value.Length == 2)
                            _positions[kv.Key] = (kv.Value[0], kv.Value[1]);
                    }
                }
            }
            catch { }
        }

        private static void TrySave()
        {
            if (_isSaveConfig)
                Save();
        }

        private class SettingsData
        {
            public bool IsTopmost { get; set; } = true;
            public bool IsPositionMemory { get; set; } = true;
            public bool IsSaveConfig { get; set; } = true;
            public Dictionary<string, double[]> Positions { get; set; }
        }
    }
}
