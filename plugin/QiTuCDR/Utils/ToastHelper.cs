using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// 轻量弹窗工具：无标题栏、无按钮、1500ms 自动消失。
    /// AllowsTransparency=true 消除 Win32 非客户区黑色背景残留。
    /// 全程代码内联 Style/UI（适配 CDR wpfhost 环境 Application.Current=null）。
    /// </summary>
    public static class ToastHelper
    {
        public static void ShowDirect(string message, bool shake = false, int durationMs = 1500)
        {
            try
            {
                var popup = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Width = 240,
                    Height = 56,
                    Background = Brushes.Transparent,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                };

                var text = new TextBlock
                {
                    Text = message,
                    FontFamily = new FontFamily("Microsoft YaHei UI"),
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xc9, 0xc9, 0xc9)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(20, 10, 20, 10),
                    Child = text,
                };

                popup.Content = border;
                popup.Show();

                if (shake)
                    ApplyShake(popup);

                StartAutoClose(popup, durationMs);
            }
            catch { }
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
    }
}
