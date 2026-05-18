# 标题栏图标 StrokeThickness 瘦身计划

## 现状
- 4 个标题栏图标（置顶/设置/展开收起/关闭）的 `StrokeThickness` 均为 **4**
- 涉及文件：`FeatureView.xaml`、`TextToCurvesView.xaml`（共 8 处）
- 图标显示尺寸：14×14（置顶/设置/展开收起）、12×12（关闭）

## 目标
将 `StrokeThickness` 从 `4` 改为 `2`，使小尺寸图标线条更清爽。

## 实施步骤

| # | 操作 | 文件 |
|---|------|------|
| 1 | 4 处 `StrokeThickness="4"` → `"2"` | `Views/FeatureView.xaml` |
| 2 | 4 处 `StrokeThickness="4"` → `"2"` | `Views/TextToCurvesView.xaml` |
| 3 | 编译 `MSBuild Clean,Build Debug x64` | — |
| 4 | 验证 DLL 已部署到 V26 Addons | — |
