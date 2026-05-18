namespace QiTuCDR.Api
{
    /// <summary>
    /// Web 后端 API 端点常量定义。
    /// 集中管理所有 API URL，便于环境切换。
    /// </summary>
    public static class ApiEndpoints
    {
        /// <summary>
        /// API 基础地址（开发环境），后续迭代中移入配置文件。
        /// </summary>
        public const string BaseUrl = "https://localhost/wp-json/qitucdr/v1";

        // ===== 认证 =====
        public const string AuthLogin    = "/auth/login";
        public const string AuthRegister = "/auth/register";
        public const string AuthRefresh  = "/auth/refresh";

        // ===== 用户 =====
        public const string UserProfile   = "/user/profile";
        public const string UserUpdate    = "/user/update";

        // ===== 订阅 =====
        public const string SubscriptionStatus  = "/subscription/status";
        public const string SubscriptionPlans   = "/subscription/plans";

        // ===== 支付 =====
        public const string PaymentCreate   = "/payment/create";
        public const string PaymentQuery    = "/payment/query";

        // ===== 同步 =====
        public const string SyncUpload   = "/sync/upload";
        public const string SyncDownload = "/sync/download";

        // ===== AI =====
        public const string LlmRewrite  = "/llm/rewrite";
        public const string LlmLayout   = "/llm/layout";
        public const string LlmColor    = "/llm/color";
    }
}
