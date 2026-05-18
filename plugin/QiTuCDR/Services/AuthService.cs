namespace QiTuCDR.Services
{
    /// <summary>
    /// 认证服务（占位，M2 S07 阶段实现）。
    /// 负责用户登录、注册、Token 管理。
    /// </summary>
    public class AuthService
    {
        // TODO: M2 S07 实现完整登录/注册逻辑

        /// <summary>
        /// 当前登录状态。
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// 当前登录用户的邮箱。
        /// </summary>
        public string CurrentUserEmail { get; private set; }

        /// <summary>
        /// 登录（待实现）。
        /// </summary>
        public void Login(string email, string password)
        {
            // TODO: 调用 ApiClient.Post(ApiEndpoints.AuthLogin, ...)
            // TODO: TokenManager.Store(token)
            // TODO: 更新 IsLoggedIn 状态
        }

        /// <summary>
        /// 登出。
        /// </summary>
        public void Logout()
        {
            // TODO: TokenManager.Clear()
            IsLoggedIn = false;
            CurrentUserEmail = null;
        }
    }
}
