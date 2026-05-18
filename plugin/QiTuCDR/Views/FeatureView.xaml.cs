using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QiTuCDR;
using QiTuCDR.Utils;

namespace QiTuCDR.Views
{
    public partial class FeatureView : UserControl
    {
        private bool _isPinned = true;
        private bool _isCollapsed;
        private double _expandedHeight = 480d;
        private readonly List<Window> _openPopups = new List<Window>();

        public FeatureView(string featureName)
        {
            InitializeComponent();
            FeatureTitleBlock.Text = featureName;
            ContentTitleBlock.Text = featureName;
            TxtVersion.Text = "V1.0";
        }

        /// <summary>
        /// 切换窗口置顶状态：通过 Win32 SetWindowLongPtr(GWL_HWNDPARENT)
        /// 将窗口绑定到 CDR 主窗口，实现仅 CDR 内部置顶，不影响其他应用。
        /// </summary>
        private void OnPin_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            if (owner == null) return;
            _isPinned = !_isPinned;
            ApplyTopmost(owner, _isPinned);
        }

        /// <summary>
        /// 根据置顶状态调用 SetCdrOwner/ClearCdrOwner 并同步钉选图标。
        /// </summary>
        private void ApplyTopmost(Window window, bool pinned)
        {
            if (pinned)
                AddonEntry.SetCdrOwner(window);
            else
                AddonEntry.ClearCdrOwner(window);

            PinIcon.Fill = pinned
                ? new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))
                : Brushes.Transparent;
        }

        /// <summary>
        /// 切换窗口展开/收起状态。收起时窗口自适应为仅标题栏高度，展开时恢复原始高度。
        /// </summary>
        private void OnCollapse_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            if (owner == null) return;

            _isCollapsed = !_isCollapsed;

            if (_isCollapsed)
            {
                if (owner.Height > 100)
                    _expandedHeight = owner.Height;
                CollapseContent();
                owner.MinHeight = 0;
                owner.SizeToContent = SizeToContent.Height;
                CollapseIcon.RenderTransformOrigin = new Point(0.5, 0.5);
                CollapseIcon.RenderTransform = new RotateTransform(180);
            }
            else
            {
                owner.SizeToContent = SizeToContent.Manual;
                owner.MinHeight = 300;
                ExpandContent();
                owner.Height = _expandedHeight;
                CollapseIcon.RenderTransform = null;
            }
        }

        private void ExpandContent()
        {
            ContentScrollViewer.Visibility = Visibility.Visible;
            ContentRow.Height = new GridLength(1, GridUnitType.Star);
        }

        private void CollapseContent()
        {
            ContentScrollViewer.Visibility = Visibility.Collapsed;
            ContentRow.Height = new GridLength(0);
        }

        /// <summary>
        /// 关闭主窗口前需精确控制 Win32 激活链：
        /// 1. 弹窗 Topmost=false + Hide() 阻断激活劫持；
        /// 2. SetWindowPos(HWND_TOP)+SetForegroundWindow 将 CDR 置顶并激活
        ///    （仅调 Z-order 和焦点，不改变最大化/最小化状态）；
        /// 3. 关闭主窗口（Windows 将激活还给 Z 序顶部的 CDR）；
        /// 4. 静默清理已隐藏的弹窗。
        /// </summary>
        private void OnClose_Click(object sender, RoutedEventArgs e)
        {
            foreach (var popup in _openPopups.ToList())
            {
                try
                {
                    popup.Topmost = false;
                    popup.Hide();
                }
                catch { }
            }

            var cdrHwnd = AddonEntry.CdrMainWindowHandle;
            if (cdrHwnd != IntPtr.Zero)
            {
                NativeMethods.SetWindowPos(cdrHwnd, (IntPtr)NativeMethods.HWND_TOP,
                    0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                NativeMethods.SetForegroundWindow(cdrHwnd);
            }

            var owner = Window.GetWindow(this);
            owner?.Close();

            foreach (var popup in _openPopups.ToList())
            {
                try { popup.Close(); } catch { }
            }
            _openPopups.Clear();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            var owner = Window.GetWindow(this);
            if (owner != null && e.ClickCount == 1)
                owner.DragMove();
        }

        /// <summary>
        /// 追踪并注册弹窗的 Closed 事件，确保关闭时从 _openPopups 移除。
        /// </summary>
        private void TrackPopup(Window popup)
        {
            _openPopups.Add(popup);
            popup.Closed += (s, args) => _openPopups.Remove(popup);
        }

        /// <summary>
        /// 打开设置弹窗（独立顶级窗口，不改变主界面收起状态）。
        /// CheckBox 变更时实时应用到主窗口并持久化。
        /// </summary>
        private void OnSettings_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);

            var popup = new Window
            {
                Title = "插件附加设置",
                Width = 300, Height = 210,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = Brushes.White,
                AllowsTransparency = false,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual,
            };

            if (mainWindow != null)
            {
                popup.Left = mainWindow.Left + (mainWindow.Width - popup.Width) / 2;
                popup.Top = mainWindow.Top + (mainWindow.Height - popup.Height) / 2;
            }
            else
            {
                var wa = SystemParameters.WorkArea;
                popup.Left = wa.Left + (wa.Width - popup.Width) / 2;
                popup.Top = wa.Top + (wa.Height - popup.Height) / 2;
            }

            TrackPopup(popup);

            var outerBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xc9, 0xc9, 0xc9)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var titlebar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf2, 0xf2, 0xf2)),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd)),
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            var titleGrid = new Grid();
            var titleText = new TextBlock
            {
                Text = "插件附加设置",
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
            };
            var closeBtn = Utils.CloseButtonFactory.Create(() => popup.Close());
            titleGrid.Children.Add(titleText);
            titleGrid.Children.Add(closeBtn);
            titleGrid.MouseLeftButtonDown += (s, args) =>
            {
                if (args.ChangedButton == MouseButton.Left)
                    popup.DragMove();
            };
            titlebar.Child = titleGrid;

            var contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf5, 0xf5, 0xf5)),
                CornerRadius = new CornerRadius(0, 0, 6, 6),
                Padding = new Thickness(20),
            };

            var checkStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var chkTopmost = new CheckBox
            {
                Content = "窗口置顶",
                IsChecked = PluginSettings.IsTopmost,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = Cursors.Hand,
            };
            chkTopmost.Checked += (s, args) =>
            {
                PluginSettings.IsTopmost = true;
                if (mainWindow != null) ApplyTopmost(mainWindow, true);
            };
            chkTopmost.Unchecked += (s, args) =>
            {
                PluginSettings.IsTopmost = false;
                if (mainWindow != null) ApplyTopmost(mainWindow, false);
            };
            checkStack.Children.Add(chkTopmost);

            var chkPosMemory = new CheckBox
            {
                Content = "窗口位置记忆",
                IsChecked = PluginSettings.IsPositionMemory,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = Cursors.Hand,
            };
            chkPosMemory.Checked += (s, args) =>
            {
                PluginSettings.IsPositionMemory = true;
            };
            chkPosMemory.Unchecked += (s, args) =>
            {
                PluginSettings.IsPositionMemory = false;
            };
            checkStack.Children.Add(chkPosMemory);

            var chkSaveConfig = new CheckBox
            {
                Content = "保存配置参数",
                IsChecked = PluginSettings.IsSaveConfig,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = Cursors.Hand,
            };
            chkSaveConfig.Checked += (s, args) =>
            {
                PluginSettings.IsSaveConfig = true;
            };
            chkSaveConfig.Unchecked += (s, args) =>
            {
                PluginSettings.IsSaveConfig = false;
            };
            checkStack.Children.Add(chkSaveConfig);

            contentBorder.Child = checkStack;

            Grid.SetRow(titlebar, 0);
            Grid.SetRow(contentBorder, 1);
            grid.Children.Add(titlebar);
            grid.Children.Add(contentBorder);
            outerBorder.Child = grid;
            popup.Content = outerBorder;

            popup.Show();
        }

        /// <summary>
        /// 打开版本信息弹窗（独立顶级窗口，不改变主界面收起状态）。
        /// </summary>
        private void OnVersion_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var mainWindow = Window.GetWindow(this);

            var popup = new Window
            {
                Title = "版本信息",
                Width = 300, Height = 200,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = Brushes.White,
                AllowsTransparency = false,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual,
            };

            if (mainWindow != null)
            {
                popup.Left = mainWindow.Left + (mainWindow.Width - popup.Width) / 2;
                popup.Top = mainWindow.Top + (mainWindow.Height - popup.Height) / 2;
            }
            else
            {
                var wa = SystemParameters.WorkArea;
                popup.Left = wa.Left + (wa.Width - popup.Width) / 2;
                popup.Top = wa.Top + (wa.Height - popup.Height) / 2;
            }

            TrackPopup(popup);

            var outerBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xc9, 0xc9, 0xc9)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var titlebar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf2, 0xf2, 0xf2)),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd)),
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            var titleGrid = new Grid();
            var titleText = new TextBlock
            {
                Text = "版本信息",
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
            };
            var closeBtn = Utils.CloseButtonFactory.Create(() => popup.Close());
            titleGrid.Children.Add(titleText);
            titleGrid.Children.Add(closeBtn);
            titleGrid.MouseLeftButtonDown += (s, args) =>
            {
                if (args.ChangedButton == MouseButton.Left)
                    popup.DragMove();
            };
            titlebar.Child = titleGrid;

            var contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf5, 0xf5, 0xf5)),
                CornerRadius = new CornerRadius(0, 0, 6, 6),
                Padding = new Thickness(20),
            };
            var contentStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            contentStack.Children.Add(new TextBlock
            {
                Text = "QiTuCDR 企图插件",
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Margin = new Thickness(0, 0, 0, 8),
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = "版本 V1.0",
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                Margin = new Thickness(0, 0, 0, 4),
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = "适用于 CorelDRAW Graphics Suite",
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
            });
            contentBorder.Child = contentStack;

            Grid.SetRow(titlebar, 0);
            Grid.SetRow(contentBorder, 1);
            grid.Children.Add(titlebar);
            grid.Children.Add(contentBorder);
            outerBorder.Child = grid;
            popup.Content = outerBorder;

            popup.Show();
        }
    }
}
