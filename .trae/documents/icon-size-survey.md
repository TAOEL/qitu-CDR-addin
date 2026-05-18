# 标题栏图标尺寸调研

## 弹窗标题栏（FeatureView / TextToCurvesView）

两个弹窗的标题栏图标尺寸完全一致：

| 图标 | 尺寸 |
|------|------|
| 置顶 (PinIcon) | **14×14** |
| 设置 (SettingsIcon) | **14×14** |
| 展开/收起 (CollapseIcon) | **14×14** |
| 关闭 (CloseIcon) | **12×12** |

> 关闭图标比其他三个小 2px。

## 页面标题栏（SettingsView）

SettingsView 使用 `PageContainerStyle` + `PageTitlebarStyle`，**没有关闭按钮**，也没有其他操作图标。只有页面标题文字 "设置"。

## 关于"版本弹窗"

当前代码库中不存在独立的版本弹窗 View。若用户指的是某个展示版本信息的弹窗，需要确认具体是哪个 View。

## 建议

如果用户觉得图标偏大，可统一缩小为 10×10（关闭 8×8），并保持关闭图标比其他三个小 2px 的规律。
