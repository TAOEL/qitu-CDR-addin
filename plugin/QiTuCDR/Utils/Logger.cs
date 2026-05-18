using System;
using System.IO;
using System.Text;

namespace QiTuCDR.Utils
{
    /// <summary>
    /// 日志工具 — 输出到 Debug + %LocalAppData%\QiTuCDR\Logs\plugin_debug.log。
    /// 用于运行时全链路诊断：COM连接、DataContext绑定、CDR API调用。
    /// </summary>
    public static class Logger
    {
        public enum Level { Debug, Info, Warning, Error }

        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QiTuCDR", "Logs");
        private static readonly string LogPath = Path.Combine(LogDir, "plugin_debug.log");
        private static readonly object _lock = new object();

        static Logger()
        {
            try { Directory.CreateDirectory(LogDir); }
            catch { }
        }

        public static void Debug(string message) => Write(Level.Debug, message);
        public static void Info(string message)  => Write(Level.Info, message);
        public static void Warn(string message)  => Write(Level.Warning, message);

        public static void Error(string message, Exception ex = null)
        {
            var sb = new StringBuilder(message);
            if (ex != null)
            {
                sb.AppendLine();
                sb.AppendLine($"  [Exception] {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    sb.AppendLine($"  [Inner] {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                sb.AppendLine($"  [StackTrace] {ex.StackTrace}");
            }
            Write(Level.Error, sb.ToString());
        }

        private static void Write(Level level, string message)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            System.Diagnostics.Debug.WriteLine(entry);
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(LogPath, entry + Environment.NewLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// 获取日志文件路径（供外部查看）。
        /// </summary>
        public static string GetLogPath() => LogPath;
    }
}
