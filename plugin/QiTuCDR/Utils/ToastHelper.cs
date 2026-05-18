using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// 轻量弹窗工具：无标题栏、无按钮、1000ms 自动消失。
    /// 全程代码内联 Style/UI（适配 CDR wpfhost 环境 Application.Current=null）。
    /// </summary>
    public static class ToastHelper
    {
        /// <summary>
        /// 弹出文本提示弹窗。
        /// </summary>
        /// <param name="message">显示文本</param>
        /// <param name="shake">是否附加水平抖动动画</param>
        /// <param name="durationMs">自动关闭毫秒数（默认 1000）</param>
        public static void Show(string message, bool shake = false, int durationMs = 1000)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                try
                {
                    var owner = GetMainWindow();
                    var popup = BuildWindow(owner);
                    var content = BuildContent(message);
                    popup.Content = content;
                    popup.Show();

                    if (shake)
                        ApplyShake(popup);

                    StartAutoClose(popup, durationMs);
                }
                catch { /* Toast 失败不抛异常 */ }
            });
        }

        /// <summary>
        /// 直接从 Dispatcher 线程显示（CDR wpfhost 中 Application.Current 为 null）。
        /// 由 ViewModel 调用时走此路径。
        /// </summary>
        public static void ShowDirect(string message, bool shake = false, int durationMs = 1000)
        {
            try
            {
                var popup = BuildWindow(null);
                var content = BuildContent(message);
                popup.Content = content;
                popup.Show();

                if (shake)
                    ApplyShake(popup);

                StartAutoClose(popup, durationMs);
            }
            catch { }
        }

        private static Window BuildWindow(Window owner)
        {
            return new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = false,
                ShowInTaskbar = false,
                Topmost = true,
                Width = 240,
                Height = 56,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Owner = owner,
            };
        }

        private static Border BuildContent(string message)
        {
            var text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xc9, 0xc9, 0xc9)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(20, 10, 20, 10),
                Child = text,
            };
        }

        private static void ApplyShake(Window popup)
        {
            popup.Loaded += (s, e) =>
            {
                var transform = new TranslateTransform();
                popup.RenderTransform = transform;

                var anim = new DoubleAnimation
                {
                    From = 0,
                    To = 6,
                    Duration = TimeSpan.FromMilliseconds(45),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(3),
                };
                transform.BeginAnimation(TranslateTransform.XProperty, anim);
            };
        }

        private static void StartAutoClose(Window popup, int durationMs)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs),
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                try { popup.Close(); } catch { }
            };
            timer.Start();
        }

        private static Window GetMainWindow()
        {
            foreach (Window w in Application.Current?.Windows)
            {
                if (w.IsVisible)
                    return w;
            }
            return null;
        }
    }
}
