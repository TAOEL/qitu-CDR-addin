using System.Windows;
using QiTuCDR.ViewModels;

namespace QiTuCDR
{
    /// <summary>
    /// QiTuCDR 主工具栏窗口。
    /// 作为 CDR 内的停靠面板，提供所有功能的导航和操作入口。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 绑定 ViewModel
            DataContext = new MainViewModel();

            // 窗口加载完成后设置所有者（由 CDR 宿主传入）
            Loaded += OnLoaded;
        }

        /// <summary>
        /// 窗口加载后，尝试将 CDR 主窗口设为此窗口的所有者，
        /// 使插件面板跟随 CDR 窗口行为（最小化/恢复/关闭）。
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 尝试绑定 CDR 主窗口为所有者（后续 S04 适配器中实现）
            // SetOwnerFromCdrHost();
        }
    }
}
