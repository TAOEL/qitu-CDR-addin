using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;

namespace QiTuCDR.Utils
{
    public class CdrV26Strategy : ICdrVersionStrategy
    {
        private object _cdrApp;
        private string _detectedVersion;
        private bool _initialized;

        public string Version => string.IsNullOrEmpty(_detectedVersion) ? "26.0" : _detectedVersion;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_detectedVersion))
                    return "CorelDRAW (未知版本)";
                return $"CorelDRAW (V{_detectedVersion})";
            }
        }

        public CdrV26Strategy()
        {
            DetectVersion();
            TryInitialize();
        }

        private void DetectVersion()
        {
            string path = null;

            try { path = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName; }
            catch { }

            if (string.IsNullOrEmpty(path))
            {
                try { path = System.Reflection.Assembly.GetEntryAssembly()?.Location; }
                catch { }
            }

            if (!string.IsNullOrEmpty(path))
            {
                var match = Regex.Match(path, @"CorelDRAW Graphics Suite\\(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    _detectedVersion = match.Groups[1].Value;
            }

            if (string.IsNullOrEmpty(_detectedVersion))
                _detectedVersion = "26";
        }

        private void TryInitialize()
        {
            _initialized = true;
            _cdrApp = TryGetCdrApp();
        }

        public object GetApplication()
        {
            if (_cdrApp != null)
            {
                try
                {
                    dynamic app = _cdrApp;
                    var _ = app.ActiveDocument;
                    return _cdrApp;
                }
                catch
                {
                    Logger.Info("[CdrV26] 缓存的 Application 已失效，重新获取");
                    _cdrApp = null;
                }
            }

            _cdrApp = TryGetCdrApp();
            return _cdrApp;
        }

        public object GetActiveDocument()
        {
            try
            {
                dynamic app = GetApplication();
                return app?.ActiveDocument;
            }
            catch
            {
                return null;
            }
        }

        public object GetSelection()
        {
            try
            {
                dynamic doc = GetActiveDocument();
                return doc?.Selection;
            }
            catch
            {
                return null;
            }
        }

        public void AttachToHost(IntPtr pluginWindowHandle)
        {
            try
            {
                dynamic app = GetApplication();
                if (app != null)
                {
                    IntPtr cdrHandle = (IntPtr)app.MainWindowHandle;
                    if (cdrHandle != IntPtr.Zero && pluginWindowHandle != IntPtr.Zero)
                    {
                        NativeMethods.SetWindowLong(
                            pluginWindowHandle,
                            NativeMethods.GWL_HWNDPARENT,
                            cdrHandle.ToInt32());
                    }
                }
            }
            catch { }
        }

        private static object TryGetCdrApp()
        {
            object result;

            result = TryGetActiveObject("CorelDRAW.Application.26");
            if (result != null) { Logger.Info("[CdrV26] ProgID 'CorelDRAW.Application.26' 连接成功"); return result; }

            result = TryGetActiveObject("CorelDRAW.Graphic.26");
            if (result != null) { Logger.Info("[CdrV26] ProgID 'CorelDRAW.Graphic.26' 连接成功"); return result; }

            result = TryGetActiveObject("CorelDRAW.Application");
            if (result != null) { Logger.Info("[CdrV26] ProgID 'CorelDRAW.Application' 连接成功"); return result; }

            result = TryGetActiveObject("CorelDRAW.Application.25");
            if (result != null) { Logger.Info("[CdrV26] ProgID 'CorelDRAW.Application.25' 连接成功（回退）"); return result; }

            Logger.Info("[CdrV26] GetActiveObject 全部失败，尝试 ROT 枚举...");

            result = TryGetCdrFromROT();
            if (result != null) { Logger.Info("[CdrV26] ROT 枚举找到 CDR Application"); return result; }

            Logger.Info("[CdrV26] ROT 枚举未找到，尝试进程内激活...");

            result = TryGetCdrInProcess();
            if (result != null) { Logger.Info("[CdrV26] 进程内激活成功"); return result; }

            Logger.Error("[CdrV26] 所有三层 Fallback 均失败 — 无法连接到 CDR");
            return null;
        }

        private static object TryGetActiveObject(string progId)
        {
            try
            {
                object obj = Marshal.GetActiveObject(progId);
                if (obj != null)
                {
                    dynamic d = obj;
                    try { var _ = d.ActiveDocument; }
                    catch
                    {
                        Logger.Info($"[CdrV26] ProgID '{progId}' 返回对象但 ActiveDocument 不可访问");
                        return null;
                    }
                }
                return obj;
            }
            catch
            {
                return null;
            }
        }

        private static object TryGetCdrFromROT()
        {
            try
            {
                GetRunningObjectTable(0, out IRunningObjectTable rot);
                rot.EnumRunning(out IEnumMoniker enumMoniker);

                IMoniker[] monikers = new IMoniker[1];
                while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
                {
                    try
                    {
                        rot.GetObject(monikers[0], out object obj);
                        if (obj != null)
                        {
                            dynamic d = obj;
                            try
                            {
                                var doc = d.ActiveDocument;
                                if (doc != null)
                                {
                                    try
                                    {
                                        string name = d.Name as string;
                                        if (!string.IsNullOrEmpty(name) &&
                                            name.IndexOf("CorelDRAW", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            return obj;
                                        }
                                    }
                                    catch { }
                                    return obj;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (monikers[0] != null)
                            Marshal.ReleaseComObject(monikers[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"[CdrV26] ROT 枚举异常: {ex.Message}");
            }
            return null;
        }

        private static object TryGetCdrInProcess()
        {
            string[] progIds = { "CorelDRAW.Application.26", "CorelDRAW.Application", "CorelDRAW.Graphic.26", "CorelDRAW.Application.25" };

            foreach (string progId in progIds)
            {
                try
                {
                    Type type = Type.GetTypeFromProgID(progId);
                    if (type != null)
                    {
                        object obj = Activator.CreateInstance(type);
                        if (obj != null)
                        {
                            dynamic d = obj;
                            try
                            {
                                var doc = d.ActiveDocument;
                                return obj;
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info($"[CdrV26] 进程内激活 '{progId}' 失败: {ex.Message}");
                }
            }

            return null;
        }

        [DllImport("ole32.dll", PreserveSig = false)]
        private static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}
