namespace QiTuCDR.Models
{
    /// <summary>
    /// 订阅数据模型（占位，M2 阶段完善）。
    /// </summary>
    public class Subscription
    {
        /// <summary>订阅 ID</summary>
        public int Id { get; set; }

        /// <summary>套餐名称</summary>
        public string PlanName { get; set; }

        /// <summary>套餐级别</summary>
        public string Level { get; set; }

        /// <summary>开始日期</summary>
        public System.DateTime StartDate { get; set; }

        /// <summary>结束日期</summary>
        public System.DateTime EndDate { get; set; }

        /// <summary>是否自动续费</summary>
        public bool AutoRenew { get; set; }
    }
}
