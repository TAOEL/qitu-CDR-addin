namespace QiTuCDR.Models
{
    /// <summary>
    /// 插件配置数据模型（占位，M2 阶段完善）。
    /// </summary>
    public class PluginConfig
    {
        /// <summary>配置键</summary>
        public string Key { get; set; }

        /// <summary>配置值</summary>
        public string Value { get; set; }

        /// <summary>上次同步时间</summary>
        public System.DateTime? LastSyncTime { get; set; }
    }
}
