using System;
using System.Windows;
using System.Windows.Interop;
using QiTuCDR.Views;

namespace QiTuCDR.Utils
{
    internal static class WindowManager
    {
        private static Window _activeWindow;
        private static string _activeFeatureKey;

        public static void OpenFeatureWindow(string featureName)
        {
            var key = "Feature:" + featureName;
            if (ActivateIfSame(key)) return;

            CloseCurrent();

            var window = new Window
            {
                Title = $"{featureName} — 企图插件",
                Width = 420,
                Height = 480,
                MinWidth = 360,
                MinHeight = 0,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = System.Windows.Media.Brushes.White,
                AllowsTransparency = false,
                SnapsToDevicePixels = true,
                Content = new FeatureView(featureName)
            };

            if (PluginSettings.IsPositionMemory
                && PluginSettings.TryGetPosition(key, out double savedLeft, out double savedTop))
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = savedLeft;
                window.Top = savedTop;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            window.Closed += OnWindowClosed;
            window.LocationChanged += (s, e) =>
            {
                if (PluginSettings.IsPositionMemory)
                    PluginSettings.SavePosition(key, window.Left, window.Top);
            };
            window.Loaded += (s, e) =>
            {
                AddonEntry.SetCdrOwner(window);
            };
            window.Show();

            _activeWindow = window;
            _activeFeatureKey = key;
        }

        public static void OpenTextToCurvesWindow()
        {
            const string key = "TextToCurves";
            if (ActivateIfSame(key)) return;

            CloseCurrent();

            var window = new Window
            {
                Title = "文字转曲 — 企图插件",
                Width = 420,
                Height = 480,
                MinWidth = 360,
                MinHeight = 0,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = System.Windows.Media.Brushes.White,
                AllowsTransparency = false,
                SnapsToDevicePixels = true,
                Content = new TextToCurvesView()
            };

            if (PluginSettings.IsPositionMemory
                && PluginSettings.TryGetPosition(key, out double savedLeft, out double savedTop))
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = savedLeft;
                window.Top = savedTop;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            window.Closed += OnWindowClosed;
            window.LocationChanged += (s, e) =>
            {
                if (PluginSettings.IsPositionMemory)
                    PluginSettings.SavePosition(key, window.Left, window.Top);
            };
            window.Loaded += (s, e) =>
            {
                AddonEntry.SetCdrOwner(window);
            };
            window.Show();

            _activeWindow = window;
            _activeFeatureKey = key;
        }

        private static bool ActivateIfSame(string key)
        {
            if (_activeWindow != null && _activeFeatureKey == key)
            {
                var hwnd = new WindowInteropHelper(_activeWindow).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
                    NativeMethods.SetForegroundWindow(hwnd);
                }
                return true;
            }
            return false;
        }

        private static void CloseCurrent()
        {
            if (_activeWindow != null)
            {
                _activeWindow.Closed -= OnWindowClosed;
                _activeWindow.Close();
                _activeWindow = null;
                _activeFeatureKey = null;
            }
        }

        private static void OnWindowClosed(object sender, EventArgs e)
        {
            _activeWindow = null;
            _activeFeatureKey = null;
        }
    }
}
