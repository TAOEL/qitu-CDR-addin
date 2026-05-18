namespace QiTuCDR.Services
{
    /// <summary>
    /// HTTP API 客户端封装（占位，M2 S07 阶段实现）。
    /// 封装 GET/POST 请求、Token 注入、错误处理。
    /// </summary>
    public class ApiClient
    {
        // TODO: M2 S07 实现完整 HTTP 客户端

        private readonly string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        // TODO: Get<T>(endpoint)
        // TODO: Post<T>(endpoint, payload)
        // TODO: 自动附加 Bearer Token
        // TODO: 401 时自动刷新 Token
        // TODO: 网络异常降级处理
    }
}
