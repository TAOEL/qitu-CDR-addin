using System;
using System.Runtime.InteropServices;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// Win32 API P/Invoke 封装。
    /// 用于插件窗口与 CDR 宿主窗口的交互。
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// 设置窗口的扩展属性（用于绑定父窗口）。
        /// </summary>
        public const int GWL_HWNDPARENT = -8;

        /// <summary>
        /// 获取指定窗口的属性值。
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 设置指定窗口的属性值。
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 设置指定窗口的属性值（64 位安全版本，推荐使用）。
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 获取指定窗口的祖先窗口。
        /// GA_ROOT=2 获取顶级根窗口。
        /// </summary>
        public const int GA_ROOT = 2;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int gaFlags);

        /// <summary>
        /// 改变窗口的 Z 序位置和状态。
        /// </summary>
        public const int HWND_TOP = 0;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, int uFlags);

        /// <summary>
        /// 查找顶级窗口（用于定位 CDR 主窗口）。
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public const int SW_RESTORE = 9;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
