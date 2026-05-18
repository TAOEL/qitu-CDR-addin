# 关闭按钮统一 + 功能窗口单例系统

## 任务 1：统一弹窗关闭按钮

**现状：** FeatureView.xaml.cs 和 TextToCurvesView.xaml.cs 中，`OnSettings_Click` 和 `OnVersion_Click` 使用内联 `TextBlock Text="×"` 作为关闭按钮，无 CloseBtn 样式（无 hover 红色效果）。

**目标：** 替换为与主标题栏完全一致的关闭按钮：
- 按钮容器：24×24，透明底，3px 圆角，hover 红色 #e81123
- 图标：8×8 SVG Path（两条对角线组成 X）
- Stroke 绑定按钮 Foreground（hover 变 White）
- 点击关闭弹窗

**修改文件：**
| # | 文件 | 位置 | 操作 |
|---|------|------|------|
| 1 | FeatureView.xaml.cs | OnSettings_Click L134-152 | 替换 TextBlock "×" → SVG X Path + CloseBtn 样式 |
| 2 | FeatureView.xaml.cs | OnVersion_Click L236-254 | 替换 TextBlock "×" → SVG X Path + CloseBtn 样式 |
| 3 | TextToCurvesView.xaml.cs | OnSettings_Click L106-123 | 替换 TextBlock "×" → SVG X Path + CloseBtn 样式 |
| 4 | TextToCurvesView.xaml.cs | OnVersion_Click L205-222 | 替换 TextBlock "×" → SVG X Path + CloseBtn 样式 |

**实现方式：** 创建 `CreateCloseButton()` 工具方法，返回已配置好 Style + Path 图标的 Button。

---

## 任务 2：全局单例窗口管理器

**架构：**
- 新建 `Utils/WindowManager.cs` — 静态类，管理全局唯一功能子窗口
- 修改 `AddonEntry.xaml.cs` — 通过 WindowManager 打开窗口

**WindowManager 职责：**
1. 维护 `_activeWindow` 引用（当前打开的功能子窗口）
2. `OpenFeatureWindow(featureName)` — 功能窗口
3. `OpenTextToCurvesWindow()` — 文字转曲窗口
4. 行为规则：
   - 打开不同功能 → 关闭旧窗口 → 打开新窗口
   - 打开相同功能 → `_activeWindow.Focus()` + `BringToFront()`

**修改文件：**
| # | 文件 | 操作 |
|---|------|------|
| 5 | `Utils/WindowManager.cs` | 新建 |
| 6 | `AddonEntry.xaml.cs` OnFeatureMenu_Click | 改用 WindowManager.OpenFeatureWindow |
| 7 | `AddonEntry.xaml.cs` OnTextToCurves_Click | 改用 WindowManager.OpenTextToCurvesWindow |

---

## 任务 3：窗口默认右侧弹出

在 WindowManager 创建窗口后，通过 Win32 `GetWindowRect` 获取 CDR 窗口矩形，计算右侧位置：
- `Left = cdrRect.Right - WindowWidth - 20`（20px 右边距）
- `Top = cdrRect.Top + 80`（略低于标题栏）
- `WindowStartupLocation = Manual`
- `WindowManager` 初始化时缓存 CDR `NativeMethods.GetWindowRect`

**修改文件：**
| # | 文件 | 操作 |
|---|------|------|
| 8 | WindowManager.cs | 添加 `PositionWindowAtRight()` 方法 |

---

## 任务 4：收起后鼠标悬浮预览

在 FeatureView.xaml.cs 和 TextToCurvesView.xaml.cs 的 `OnCollapse_Click` 中，当收起时：
- 用 `VisualBrush` 截取内容区快照
- 设为标题栏的 `ToolTip`（展示快照预览）
- 展开时清除 ToolTip

**修改文件：**
| # | 文件 | 操作 |
|---|------|------|
| 9 | FeatureView.xaml.cs OnCollapse_Click | 收起时设置标题栏 ToolTip = VisualBrush 快照 |
| 10 | TextToCurvesView.xaml.cs OnCollapse_Click | 收起时设置标题栏 ToolTip = VisualBrush 快照 |

---

## 任务 5：编译部署 V26 验证
