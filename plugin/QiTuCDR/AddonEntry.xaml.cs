using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using QiTuCDR.Utils;
using QiTuCDR.ViewModels;
using QiTuCDR.Views;

namespace QiTuCDR
{
    /// <summary>
    /// CDR Addon 横向工具栏入口 UserControl。
    /// 由 CorelDRAW 框架通过 wpfhost 加载为横向工具条。
    /// </summary>
    public partial class AddonEntry : UserControl
    {
        private ToolbarViewModel _viewModel;

        /// <summary>
        /// CDR 主窗口句柄，通过 PresentationSource 可视树 + GetAncestor(GA_ROOT)
        /// 精准获取 CDR 顶级窗口句柄，确保 SetWindowLongPtr(GWL_HWNDPARENT)
        /// 正确绑定子窗口到真实的 CDR 文档窗口。
        /// </summary>
        internal static IntPtr CdrMainWindowHandle
        {
            get
            {
                if (_cdrMainWindowHandle == IntPtr.Zero)
                {
                    try
                    {
                        _cdrMainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
                    }
                    catch { }
                }
                return _cdrMainWindowHandle;
            }
        }
        private static IntPtr _cdrMainWindowHandle;

        /// <summary>
        /// 通过 Win32 SetWindowLongPtr(GWL_HWNDPARENT) 将 WPF 窗口挂载为 CDR 主窗口的子窗口，
        /// 实现仅在 CDR 内部置顶，离开 CDR 后不强制置顶其他程序。
        /// 在 window.Loaded 事件中调用（window 完全初始化后），并在 SetWindowLongPtr 之后
        /// 立即调用 SetWindowPos 强制窗口管理器更新 Z-order 关系。
        /// </summary>
        internal static void SetCdrOwner(Window window)
        {
            var cdrHandle = CdrMainWindowHandle;
            if (cdrHandle == IntPtr.Zero) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_HWNDPARENT, cdrHandle);
            NativeMethods.SetWindowPos(hwnd, (IntPtr)NativeMethods.HWND_TOP,
                0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        }

        /// <summary>
        /// 解除窗口与 CDR 主窗口的父子绑定，恢复普通窗口行为。
        /// </summary>
        internal static void ClearCdrOwner(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
                NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_HWNDPARENT, IntPtr.Zero);
        }

        /// <summary>
        /// 初始化横向工具栏，绑定 ToolbarViewModel，并在 Loaded 时缓存 CDR 窗口句柄。
        /// </summary>
        public AddonEntry()
        {
            InitializeComponent();

            _viewModel = new ToolbarViewModel();
            DataContext = _viewModel;

            AppResources.Initialize();

            Loaded += OnAddonEntryLoaded;
        }

        /// <summary>
        /// 控件加载后通过可视树精准获取 CDR 顶级窗口句柄。
        /// 使用 GetAncestor(GA_ROOT) 从 wpfhost 向上遍历到 CDR 文档主窗口。
        /// </summary>
        private void OnAddonEntryLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                if (hwndSource != null && hwndSource.Handle != IntPtr.Zero)
                {
                    var rootHandle = NativeMethods.GetAncestor(hwndSource.Handle, NativeMethods.GA_ROOT);
                    if (rootHandle != IntPtr.Zero)
                        _cdrMainWindowHandle = rootHandle;
                }
            }
            catch { }
        }

        /// <summary>
        /// 工具栏分类按钮点击：切换下拉菜单，互斥关闭其他。
        /// </summary>
        private void ToggleCategory_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton btn) || btn.Tag == null)
                return;

            string category = btn.Tag.ToString();

            _viewModel.IsBrandOpen = category == "Brand" && btn.IsChecked == true;
            _viewModel.IsHomeOpen = category == "Home" && btn.IsChecked == true;
            _viewModel.IsLayoutOpen = category == "Layout" && btn.IsChecked == true;
            _viewModel.IsGraphicsOpen = category == "Graphics" && btn.IsChecked == true;
            _viewModel.IsTextOpen = category == "Text" && btn.IsChecked == true;
            _viewModel.IsColorOpen = category == "Color" && btn.IsChecked == true;
            _viewModel.IsAIOpen = category == "AI" && btn.IsChecked == true;
        }

        /// <summary>
        /// 打开文字转曲独立窗口（Win32 GWLP_HWNDPARENT 绑定 CDR 宿主，置顶仅限 CDR 内部）。
        /// </summary>
        private void OnTextToCurves_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenTextToCurvesWindow();
        }

        /// <summary>
        /// 通用功能菜单点击：读取按钮内容作为功能名称，打开 FeatureView 窗口（Win32 GWLP_HWNDPARENT 绑定 CDR 宿主，置顶仅限 CDR 内部）。
        /// </summary>
        private void OnFeatureMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            string featureName = btn.Content?.ToString() ?? "功能";
            if (string.IsNullOrWhiteSpace(featureName)) featureName = "未命名功能";

            WindowManager.OpenFeatureWindow(featureName);
        }
    }
}
