namespace QiTuCDR.Models
{
    /// <summary>
    /// 用户数据模型（占位，M2 阶段完善）。
    /// </summary>
    public class User
    {
        /// <summary>用户 ID</summary>
        public int Id { get; set; }

        /// <summary>邮箱</summary>
        public string Email { get; set; }

        /// <summary>昵称</summary>
        public string Nickname { get; set; }

        /// <summary>头像 URL</summary>
        public string AvatarUrl { get; set; }

        /// <summary>订阅状态（free / premium / expired）</summary>
        public string SubscriptionStatus { get; set; }

        /// <summary>订阅到期时间</summary>
        public System.DateTime? SubscriptionEndDate { get; set; }
    }
}
