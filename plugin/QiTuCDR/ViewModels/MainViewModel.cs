using System.Collections.Generic;
using System.Windows.Controls;
using QiTuCDR.Views;

namespace QiTuCDR.ViewModels
{
    /// <summary>
    /// 主 ViewModel。
    /// 管理工具栏下拉状态、页面导航、全局状态。
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private UserControl _currentView;
        private string _statusText;
        private string _currentNavItem;

        private bool _isHomeOpen;
        private bool _isLayoutOpen;
        private bool _isGraphicsOpen;
        private bool _isTextOpen;
        private bool _isColorOpen;
        private bool _isAIOpen;

        private readonly Dictionary<string, (string Name, UserControl View)> _navMap;

        public MainViewModel()
        {
            _navMap = new Dictionary<string, (string, UserControl)>
            {
                { "Home",     ("工具面板", new HomeView()) },
                { "Layout",   ("排版工具", new LayoutView()) },
                { "Graphics", ("图形工具", new GraphicsView()) },
                { "Text",     ("文字工具", new TextToCurvesView()) },
                { "Color",    ("颜色工具", new ColorView()) },
                { "AI",       ("AI 助手",  new AIView()) },
                { "Settings", ("设置",     new SettingsView()) },
            };

            NavigateCommand = new RelayCommand(OnNavigate);
            _statusText = "就绪";
            NavigateTo("Home");
        }

        /// <summary>导航命令。</summary>
        public RelayCommand NavigateCommand { get; }

        /// <summary>工具栏下拉开关状态。</summary>
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

        /// <summary>当前视图。</summary>
        public UserControl CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        /// <summary>状态文本。</summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>当前导航项。</summary>
        public string CurrentNavItem
        {
            get => _currentNavItem;
            set => SetProperty(ref _currentNavItem, value);
        }

        /// <summary>
        /// 导航到指定页面。
        /// </summary>
        public void NavigateTo(string navKey)
        {
            if (_navMap.TryGetValue(navKey, out var item))
            {
                CurrentView = item.View;
                CurrentNavItem = navKey;
                StatusText = item.Name;
            }
        }

        private void OnNavigate(object parameter)
        {
            if (parameter is string navKey)
                NavigateTo(navKey);
        }
    }
}
