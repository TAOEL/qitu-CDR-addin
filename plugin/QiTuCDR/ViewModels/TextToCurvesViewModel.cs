using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using QiTuCDR.Utils;

namespace QiTuCDR.ViewModels
{
    /// <summary>
    /// 智能批量转曲 ViewModel。
    /// 
    /// COM 互操作：全部属性访问走 ComGet（RCW 反射），dynamic/IDispatch 不可用。
    /// 
    /// CDR 官方 Shape.Type 枚举 (cdrShapeType v26)：
    ///   0=NoShape  1=Rectangle  2=Ellipse  3=CurveShape(曲线!)
    ///   4=Polygon  5=Bitmap     6=Text     7=Group
    ///   19=Connector  21=CustomShape  26=PerfectShape
    /// 
    /// 图形判定 (IsPresetShape)：仅 1/2/4/19/21/26，强制排除 3(曲线)和 5(位图)
    /// 转曲策略：
    ///   文本(6) → shape.ConvertToCurves()（Ctrl+Q）
    ///   图形(1,2,4,19,21,26) → shape.ConvertToCurves()（Ctrl+Q）
    ///   轮廓(任意type+Outline.Width>0) → shape.Outline.ConvertToObject()（Ctrl+Shift+Q）
    /// </summary>
    public class TextToCurvesViewModel : ViewModelBase
    {
        private string _documentName = "（无活动文档）";
        private int _textCount;
        private int _graphicsCount;
        private int _outlineCount;

        private bool _processAllDocument;
        private bool _processSelection;
        private bool _convertText = true;
        private bool _convertGraphics = true;
        private bool _convertOutlines = true;
        private bool _isProcessing;

        private string _resultMessage;
        private bool _resultSuccess;
        private bool _hasResult;
        private int _convertedCount;
        private int _failedCount;

        public TextToCurvesViewModel()
        {
            Logger.Info("[T2C] ViewModel 构造函数");
            _logEntries = new ObservableCollection<LogEntry>();
            ExecuteCommand = new RelayCommand(OnExecute, _ => !_isProcessing);
            CopyResultCommand = new RelayCommand(OnCopyResult);
            Logger.Info("[T2C] 命令就绪，刷新文档...");
            RefreshDocumentInfo();
        }

        // ═══════════════════ 属性 ═══════════════════

        public string DocumentName { get => _documentName; set => SetProperty(ref _documentName, value); }

        public int TextCount { get => _textCount; set { if (SetProperty(ref _textCount, value)) OnPropertyChanged(nameof(TextCountInfo)); } }
        public int GraphicsCount { get => _graphicsCount; set { if (SetProperty(ref _graphicsCount, value)) OnPropertyChanged(nameof(GraphicsCountInfo)); } }
        public int OutlineCount { get => _outlineCount; set { if (SetProperty(ref _outlineCount, value)) OnPropertyChanged(nameof(OutlineCountInfo)); } }

        /// <summary>作用域前缀：页面 / 文档 / 选中。</summary>
        public string ScopePrefix => ProcessAllDocument ? "文档" : ProcessSelection ? "选中" : "页面";

        public string TextCountInfo =>
            $"{ScopePrefix} / 共 {TextCount} 个";

        public string GraphicsCountInfo =>
            $"{ScopePrefix} / 共 {GraphicsCount} 个";

        public string OutlineCountInfo =>
            $"{ScopePrefix} / 共 {OutlineCount} 个";

        public bool ProcessSelection
        {
            get => _processSelection;
            set
            {
                if (SetProperty(ref _processSelection, value) && value) ProcessAllDocument = false;
                NotifyInfoProps();
                RefreshDocumentInfo();
            }
        }
        public bool ProcessAllDocument
        {
            get => _processAllDocument;
            set
            {
                if (SetProperty(ref _processAllDocument, value) && value) ProcessSelection = false;
                NotifyInfoProps();
                RefreshDocumentInfo();
            }
        }

        private void NotifyInfoProps()
        {
            OnPropertyChanged(nameof(ScopePrefix));
            OnPropertyChanged(nameof(TextCountInfo));
            OnPropertyChanged(nameof(GraphicsCountInfo));
            OnPropertyChanged(nameof(OutlineCountInfo));
        }

        public bool ConvertText { get => _convertText; set => SetProperty(ref _convertText, value); }
        public bool ConvertGraphics { get => _convertGraphics; set => SetProperty(ref _convertGraphics, value); }
        public bool ConvertOutlines { get => _convertOutlines; set => SetProperty(ref _convertOutlines, value); }

        public bool IsProcessing { get => _isProcessing; set { if (SetProperty(ref _isProcessing, value)) ExecuteCommand.RaiseCanExecuteChanged(); } }
        public string ResultMessage { get => _resultMessage; set => SetProperty(ref _resultMessage, value); }
        public bool ResultSuccess { get => _resultSuccess; set => SetProperty(ref _resultSuccess, value); }
        public bool HasResult { get => _hasResult; set => SetProperty(ref _hasResult, value); }
        public int ConvertedCount { get => _convertedCount; set => SetProperty(ref _convertedCount, value); }
        public int FailedCount { get => _failedCount; set => SetProperty(ref _failedCount, value); }

        public ObservableCollection<LogEntry> LogEntries => _logEntries;
        private readonly ObservableCollection<LogEntry> _logEntries;
        public RelayCommand ExecuteCommand { get; }
        public RelayCommand CopyResultCommand { get; }

        // ═══════════════════ COM 互操作 ═══════════════════

        private static readonly BindingFlags _cf = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
        private static readonly BindingFlags _mf = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
        private object _cachedApp;

        private static object ComGet(object obj, string name) => obj.GetType().InvokeMember(name, _cf, null, obj, null);

        /// <summary>
        /// COM 集合枚举器。ActiveSelection/Shapes 等 COM 对象不一定实现 .NET IEnumerable，
        /// 需要依次尝试：_NewEnum (IEnumVARIANT) → GetEnumerator → IEnumerable 回退。
        /// 返回 List&lt;object&gt; 保证可安全遍历。
        /// </summary>
        private static List<object> ComEnumerate(object collection)
        {
            var result = new List<object>();
            if (collection == null) return result;

            // 1) .NET IEnumerable（最快）
            if (collection is System.Collections.IEnumerable ie)
            {
                try { foreach (var item in ie) result.Add(item); } catch { }
                if (result.Count > 0) return result;
            }

            // 2) COM _NewEnum → IEnumVARIANT（标准 COM 集合 0 协议）
            try
            {
                var t = collection.GetType();
                object enumerator = t.InvokeMember("_NewEnum",
                    BindingFlags.GetProperty | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                    null, collection, null);
                if (enumerator != null)
                {
                    // IEnumVARIANT 有 Next 方法
                    var enumType = enumerator.GetType();
                    var nextMethod = enumType.GetMethod("Next");
                    if (nextMethod == null)
                    {
                        // fallback: InvokeMember
                        object[] args = new object[] { 1, null, null };
                        while (true)
                        {
                            try
                            {
                                enumType.InvokeMember("Next",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                                    null, enumerator, args);
                                if (args[1] != null) result.Add(args[1]);
                                else break;
                                args = new object[] { 1, null, null };
                            }
                            catch { break; }
                        }
                    }
                    else
                    {
                        object[] args = new object[] { 1, null, IntPtr.Zero };
                        try
                        {
                            while (nextMethod.Invoke(enumerator, args) as int? == 0 && args[1] != null)
                            {
                                result.Add(args[1]);
                                args = new object[] { 1, null, IntPtr.Zero };
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // 3) foreach via dynamic（回退）
            if (result.Count == 0)
            {
                try
                {
                    dynamic d = collection;
                    int n = 0;
                    foreach (var item in d) { result.Add(item); n++; if (n > 100000) break; }
                }
                catch { }
            }

            return result;
        }

        /// <summary>调用 COM 方法（返回 void 或 object）。</summary>
        private static void ComInvoke(object obj, string method, object[] args = null)
        {
            try { obj.GetType().InvokeMember(method, _mf, null, obj, args ?? new object[0]); }
            catch (Exception ex) { Logger.Warn($"[T2C] ComInvoke {method}: {ex.Message}"); }
        }

        private object GetApp()
        {
            if (_cachedApp != null) return _cachedApp;
            _cachedApp = CdrVersionAdapter.Instance.GetApplication();
            if (_cachedApp == null) Logger.Warn("[T2C] GetApplication → null");
            else Logger.Info("[T2C] Cache → OK");
            return _cachedApp;
        }

        // ═══════════════════ Refresh ═══════════════════

        public void RefreshDocumentInfo()
        {
            try
            {
                Logger.Info("[T2C] Refresh");
                var app = GetApp(); if (app == null) { ClearDoc(); return; }
                var doc = ComGet(app, "ActiveDocument");
                if (doc == null) { ClearDoc(); return; }
                DocumentName = (ComGet(doc, "Name") as string) ?? (ComGet(doc, "FileName") as string) ?? "未命名文档";

                int t = 0, g = 0, o = 0;

                if (ProcessSelection)
                {
                    var sel = ComGet(app, "ActiveSelection");
                    if (sel != null)
                    {
                        var selList = ComEnumerate(sel);
                        foreach (var sh in selList) CountOne((dynamic)sh, ref t, ref g, ref o);
                    }
                }
                else if (ProcessAllDocument)
                {
                    var pages = ComGet(doc, "Pages");
                    if (pages != null)
                    {
                        var pageList = ComEnumerate(pages);
                        foreach (var p in pageList)
                        {
                            var shapesObj = ComGet(p, "Shapes");
                            if (shapesObj != null)
                            {
                                var shapeList = ComEnumerate(shapesObj);
                                foreach (var sh in shapeList) CountOne((dynamic)sh, ref t, ref g, ref o);
                            }
                        }
                    }
                    else
                    {
                        var ap = ComGet(doc, "ActivePage");
                        if (ap != null) { var sl = ComEnumerate(ComGet(ap, "Shapes")); foreach (var sh in sl) CountOne((dynamic)sh, ref t, ref g, ref o); }
                    }
                }
                else // 当前页面
                {
                    var ap = ComGet(doc, "ActivePage");
                    if (ap != null) { var sl = ComEnumerate(ComGet(ap, "Shapes")); foreach (var sh in sl) CountOne((dynamic)sh, ref t, ref g, ref o); }
                }

                TextCount = t; GraphicsCount = g; OutlineCount = o;
                Logger.Info($"[T2C] 统计→ 文本:{t} 图形:{g} 轮廓:{o}");
            }
            catch (Exception ex) { ClearDoc(); Logger.Error("[T2C] Refresh异常", ex); }
        }

        private void ClearDoc() { DocumentName = "（无活动文档）"; TextCount = GraphicsCount = OutlineCount = 0; }

        // ═══════════════════ OnExecute ═══════════════════

        private void OnExecute(object _)
        {
            Logger.Info($"[T2C] ***** 转曲开始 scope={ScopeLabel} text={ConvertText} gfx={ConvertGraphics} outl={ConvertOutlines}");
            IsProcessing = true; HasResult = false; ConvertedCount = FailedCount = 0; _logEntries.Clear();
            int c = 0, f = 0; bool grouped = false;
            try
            {
                var app = GetApp(); if (app == null) { ToastHelper.ShowDirect("请打开文档", shake: true); Show(false, "CDR未连接"); return; }
                var doc = ComGet(app, "ActiveDocument");
                if (doc == null) { ToastHelper.ShowDirect("请打开文档", shake: true); Show(false, "无活动文档"); return; }

                object shapesObj = GetTargetShapes(app, doc);
                if (shapesObj == null) { Show(false, "无有效对象"); return; }

                ComBeginGroup(doc, "批量智能转曲"); grouped = true;
                ProcessShapes(shapesObj, ref c, ref f);
                ComEndGroup(doc); grouped = false;

                ConvertedCount = c; FailedCount = f;
                if (c > 0)
                {
                    ToastHelper.ShowDirect("转曲成功");
                    if (f == 0)      Show(true, $"成功转曲 {c} 个对象");
                    else             Show(true, $"转曲完成：成功{c} 失败{f}");
                }
                else if (f > 0)          Show(false, $"转曲失败：{f} 个对象");
                else                     Show(false, "未找到可转曲对象");

                RefreshDocumentInfo();
            }
            catch (Exception ex) { Show(false, ex.Message); Log("✗ 异常: " + ex.Message, LogLevel.Error); Logger.Error("转曲异常", ex); }
            finally
            {
                if (grouped) try { ComEndGroup(null); } catch { }
                IsProcessing = false;
            }
        }

        /// <summary>范围描述（用于日志）。</summary>
        private string ScopeLabel =>
            ProcessAllDocument ? "整个文档" : ProcessSelection ? "选中对象" : "当前页面";

        /// <summary>获取待处理对象集合。</summary>
        private object GetTargetShapes(object app, object doc)
        {
            if (ProcessAllDocument)      return ComGet(doc, "Pages");
            if (ProcessSelection)        return ComGet(app, "ActiveSelection");
            /* 当前页面 */               return ComGet(doc, "ActivePage");
        }

        /// <summary>递归处理 Shapes/Selection/Page。</summary>
        private void ProcessShapes(object obj, ref int c, ref int f)
        {
            if (obj == null) return;

            // 统一枚举为 List（兼容 COM 集合 + .NET IEnumerable + _NewEnum 回退）
            var list = ComEnumerate(obj);
            if (list.Count > 0)
            {
                foreach (var item in list) ProcessShapeOne(item, ref c, ref f);
                return;
            }

            // 非集合：可能是单个 Shape 或 Page（取 Shapes 再处理）
            if (IsShape(obj)) { ProcessShapeOne(obj, ref c, ref f); return; }

            var sub = ComGet(obj, "Shapes");
            if (sub != null)
            {
                var subList = ComEnumerate(sub);
                foreach (var s in subList) ProcessShapeOne(s, ref c, ref f);
            }
        }

        /// <summary>处理单个 Shape（含递归群组+PowerClip）。</summary>
        private void ProcessShapeOne(object obj, ref int c, ref int f)
        {
            try
            {
                int type = 0; try { type = (int)((dynamic)obj).Type; } catch { }
                string typeLabel = ShapeTypeLabel(type);

                // 递归群组
                var child = ComGet(obj, "Shapes");
                if (child != null && GetSafeCount(child) > 0)
                {
                    var childList = ComEnumerate(child);
                    foreach (var s in childList) ProcessShapeOne(s, ref c, ref f);
                }

                // 递归PowerClip
                var pc = ComGet(obj, "PowerClip");
                if (pc != null)
                {
                    var pcSh = ComGet(pc, "Shapes");
                    if (pcSh != null && GetSafeCount(pcSh) > 0)
                    {
                        var pcList = ComEnumerate(pcSh);
                        foreach (var s in pcList) ProcessShapeOne(s, ref c, ref f);
                    }
                }

                // 文本 cdrTextShape=6 → ConvertToCurves
                if (type == 6 && ConvertText)
                {
                    try
                    {
                        string label = TextLabel(obj);
                        Logger.Info($"  [文本] {label}");
                        ((dynamic)obj).ConvertToCurves();
                        Log($"✓ 文本转曲: {label}", LogLevel.Success); c++;
                    }
                    catch (Exception ex) { Log($"✗ 文本失败: {ex.Message}", LogLevel.Error); f++; }
                }
                // 图形类型：仅 CDR 原生预设形状 (Rect=1,Ellipse=2,Polygon=4,Connector=19,Custom=21,Perfect=26)
                // 强制排除 CurveShape=3 (曲线对象)
                else if (IsPresetShape(type) && ConvertGraphics)
                {
                    try
                    {
                        Logger.Info($"  [图形] type={type}({typeLabel})");
                        ((dynamic)obj).ConvertToCurves();
                        Log($"✓ 图形转曲 type={type}({typeLabel})", LogLevel.Success); c++;
                    }
                    catch (Exception ex) { Log($"✗ 图形失败 type={type}: {ex.Message}", LogLevel.Error); f++; }
                }
                else if (type == 3)
                {
                    // 曲线对象：明确跳过，仅记录日志
                    Logger.Info($"  [曲线-跳过] type={type}({typeLabel})");
                }

                // 轮廓 → ConvertToObject (Ctrl+Shift+Q) — 不限制 type，仅判定 Outline 存在
                if (ConvertOutlines && HasOutline(obj))
                {
                    try
                    {
                        dynamic outline = ((dynamic)obj).Outline;
                        if (outline != null)
                        {
                            Logger.Info($"  [轮廓] type={type}({typeLabel}) → ConvertToObject");
                            outline.ConvertToObject();
                            Log($"✓ 轮廓转对象 type={type}({typeLabel})", LogLevel.Success); c++;
                        }
                    }
                    catch (Exception ex) { Log($"✗ 轮廓失败: {ex.Message}", LogLevel.Error); f++; }
                }
            }
            catch (Exception ex) { Log($"✗ Shape异常: {ex.Message}", LogLevel.Error); f++; }
        }

        /// <summary>
        /// CDR 原生预设形状判定。
        /// 官方 cdrShapeType 枚举 (v26):
        ///   1=cdrRectangleShape  2=cdrEllipseShape  4=cdrPolygonShape
        ///   19=cdrConnectorShape 21=cdrCustomShape   26=cdrPerfectShape
        /// 强制排除: 3=cdrCurveShape(曲线对象), 5=cdrBitmapShape, 0/7~18/20/22~25/27
        /// </summary>
        private static bool IsPresetShape(int type)
        {
            return type == 1 || type == 2 || type == 4
                || type == 19 || type == 21 || type == 26;
        }

        /// <summary>Shape.Type 枚举值 → 中文标签（用于日志明细）。</summary>
        private static string ShapeTypeLabel(int type)
        {
            switch (type)
            {
                case 0: return "无形状";
                case 1: return "矩形";
                case 2: return "椭圆";
                case 3: return "曲线对象";       // ← 严禁识别为图形
                case 4: return "多边形";
                case 5: return "位图";
                case 6: return "文本";
                case 7: return "群组";
                case 19: return "连接线";
                case 21: return "自定义形状";
                case 26: return "完美形状";
                default: return $"类型{type}";
            }
        }

        private static bool HasOutline(object obj)
        {
            try
            {
                dynamic o = ((dynamic)obj).Outline;
                if (o == null) return false;
                double w = (double)o.Width;
                return w > 0.001;
            }
            catch { return false; }
        }

        private static bool IsShape(object obj) { try { var _ = (int)((dynamic)obj).Type; return true; } catch { return false; } }
        private static string TextLabel(object obj) { return TextLabelDynamic((dynamic)obj); }
        private static string TextLabelDynamic(dynamic s)
        {
            try {
                string t = null;
                try { t = s.TextFrame?.Story?.Text as string; } catch { }
                if (string.IsNullOrEmpty(t)) try { t = s.Text?.Story?.Text as string; } catch { }
                if (string.IsNullOrEmpty(t)) try { t = s.Text as string; } catch { }
                if (string.IsNullOrEmpty(t)) return "（无内容）";
                if (t.Length > 30) t = t.Substring(0, 27) + "...";
                try { return $"{t} [{s.Font?.Name} {s.Font?.Size:F0}pt]"; } catch { return t; }
            } catch { return "?"; }
        }

        /// <summary>统计单个 Shape（递归群组+PowerClip），输出明细日志到文件。</summary>
        private void CountOne(dynamic s, ref int t, ref int g, ref int o)
        {
            try
            {
                int ty = 0; try { ty = (int)s.Type; } catch { }
                if (ty == 6) { t++; Logger.Info($"  [检测] 文本 type=6"); }
                else if (IsPresetShape(ty)) { g++; Logger.Info($"  [检测] 图形 type={ty}({ShapeTypeLabel(ty)})"); }
                else if (ty == 3) { Logger.Info($"  [检测] 曲线-排除 type=3"); }
                else if (ty > 0) { Logger.Info($"  [检测] 其他 type={ty}({ShapeTypeLabel(ty)})"); }
                if (HasOutline(s)) { o++; Logger.Info($"  [检测] 轮廓 type={ty}"); }
                var c = ComGet(s, "Shapes"); if (c != null) { var cl = ComEnumerate(c); foreach (var ch in cl) CountOne((dynamic)ch, ref t, ref g, ref o); }
                var p = ComGet(s, "PowerClip"); if (p != null) { var pc = ComGet(p, "Shapes"); if (pc != null) { var pcl = ComEnumerate(pc); foreach (var ch in pcl) CountOne((dynamic)ch, ref t, ref g, ref o); } }
            }
            catch { }
        }

        private static int GetSafeCount(object c)
        {
            if (c == null) return 0;
            try { return (int)c.GetType().InvokeMember("Count", _cf, null, c, null); } catch { }
            try { int n = 0; foreach (var _ in (System.Collections.IEnumerable)c) { n++; if (n > 100000) break; } return n; } catch { return 0; }
        }

        private static void ComBeginGroup(object doc, string name)
        {
            try { doc.GetType().InvokeMember("BeginCommandGroup", _mf, null, doc, new object[] { name }); Logger.Info("[T2C] BeginCommandGroup OK"); }
            catch (Exception ex) { Logger.Warn($"[T2C] BeginCommandGroup不可用: {ex.Message}"); }
        }
        private static void ComEndGroup(object doc)
        {
            try { if (doc != null) { doc.GetType().InvokeMember("EndCommandGroup", _mf, null, doc, null); Logger.Info("[T2C] EndCommandGroup OK"); } }
            catch (Exception ex) { Logger.Warn($"[T2C] EndCommandGroup不可用: {ex.Message}"); }
        }

        // ═══════════════════ 结果输出 ═══════════════════

        private void Show(bool ok, string msg) { ResultSuccess = ok; ResultMessage = msg; HasResult = true; }
        private void Log(string msg, LogLevel lv) { AddLog(msg, lv); }

        private void OnCopyResult(object _)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"===== 转曲结果 {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            sb.AppendLine($"成功:{ConvertedCount} 失败:{FailedCount} 消息:{ResultMessage}");
            sb.AppendLine("------");
            foreach (var e in _logEntries) sb.AppendLine($"[{e.Time}][{e.Level}] {e.Message}");
            sb.AppendLine("======");
            Clipboard.SetText(sb.ToString());
        }

        private void AddLog(string msg, LogLevel lv)
        {
            var e = new LogEntry { Time = DateTime.Now.ToString("HH:mm:ss"), Level = lv, Message = msg };
            _logEntries.Add(e);
            try
            {
                var d = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiTuCDR", "Logs");
                System.IO.Directory.CreateDirectory(d);
                var p = System.IO.Path.Combine(d, $"TextToCurves_{DateTime.Now:yyyyMMdd}.log");
                lock (_l) { System.IO.File.AppendAllText(p, $"[{e.Time}][{lv}] {msg}\n"); }
            }
            catch { }
        }
        private static readonly object _l = new object();
    }

    internal class ConvertResult { public bool Success; public string TextPreview; public string FontName; public double FontSize; }
    public class LogEntry { public string Time { get; set; } public string Message { get; set; } public LogLevel Level { get; set; } }
    public enum LogLevel { Info, Success, Warning, Error }
}
