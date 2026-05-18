using System.Collections.Generic;
using System;

namespace QiTuCDR.ViewModels
{
    /// <summary>
    /// 工具栏专用 ViewModel。
    /// 仅管理下拉开关状态和用户信息，无 View 页面依赖。
    /// 由 AddonEntry (CDR wpfhost) 使用。
    /// </summary>
    public class ToolbarViewModel : ViewModelBase
    {
        private bool _isBrandOpen;
        private bool _isHomeOpen;
        private bool _isLayoutOpen;
        private bool _isGraphicsOpen;
        private bool _isTextOpen;
        private bool _isColorOpen;
        private bool _isAIOpen;
        private string _userStatus = "未登录";

        public bool IsBrandOpen
        {
            get => _isBrandOpen;
            set => SetProperty(ref _isBrandOpen, value);
        }

        public bool IsHomeOpen
        {
            get => _isHomeOpen;
            set => SetProperty(ref _isHomeOpen, value);
        }

        public bool IsLayoutOpen
        {
            get => _isLayoutOpen;
            set => SetProperty(ref _isLayoutOpen, value);
        }

        public bool IsGraphicsOpen
        {
            get => _isGraphicsOpen;
            set => SetProperty(ref _isGraphicsOpen, value);
        }

        public bool IsTextOpen
        {
            get => _isTextOpen;
            set => SetProperty(ref _isTextOpen, value);
        }

        public bool IsColorOpen
        {
            get => _isColorOpen;
            set => SetProperty(ref _isColorOpen, value);
        }

        public bool IsAIOpen
        {
            get => _isAIOpen;
            set => SetProperty(ref _isAIOpen, value);
        }

        /// <summary>
        /// 用户状态文字（未登录 / 用户昵称）。
        /// </summary>
        public string UserStatus
        {
            get => _userStatus;
            set => SetProperty(ref _userStatus, value);
        }
    }
}
