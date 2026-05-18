# VSTA + C# 插件开发指南

## 🎉 项目已创建完成！

我们为你创建了一个完整的 CorelDRAW VSTA 插件项目，包含：

### 📦 已创建的文件

**项目配置：**
- `CDRPowerTools.csproj` - 项目文件
- `AddIn.cs` - VSTA 插件入口

**核心服务：**
- `CorelDRAWService.cs` - CorelDRAW API 封装

**界面视图：**
- `MainWindow.xaml` - 主工具面板
- `TextInputWindow.xaml` - 文字输入窗口
- `ShapeChooserWindow.xaml` - 形状选择窗口
- `BatchExportWindow.xaml` - 批量导出窗口
- `SettingsWindow.xaml` - 设置窗口
- `AIToolsWindow.xaml` - AI工具窗口
- `AIGenerationWindow.xaml` - AI生成窗口

**模型和工具：**
- `AppSettings.cs` - 应用设置
- `Logger.cs` - 日志工具

### 🎨 设计特点

- 采用 MaterialDesign 浅色主题
- 基于 .NET 7 + WPF
- 现代化的界面设计
- 完整的日志和设置系统

---

## 📚 VSTA 开发学习路径

### 第一步：环境配置

1. **安装必需软件**
   - Visual Studio 2022 (选择 .NET 桌面开发)
   - .NET 7 SDK
   - CorelDRAW 2023 或更高版本 (安装时选择 VSTA 支持)

2. **配置项目引用**
   - 找到 `Corel.Interop.VGCore.dll` 的实际位置
   - 修改 `CDRPowerTools.csproj` 中的引用路径
   - 生成新的 GUID 并替换 `AddIn.cs` 中的占位符

### 第二步：学习 API

**推荐学习顺序：**

1. **基础概念**
   - Application 应用程序对象
   - Document 文档对象
   - Page 页面对象
   - Shape 形状对象
   - Selection 选择集

2. **常用操作**
   - 获取和修改选中的形状
   - 创建新形状
   - 修改属性（位置、大小、颜色）
   - 保存和导出

3. **进阶功能**
   - 事件处理
   - 命令栏和菜单
   - 对话框和用户界面

### 第三步：调试和测试

1. **调试配置**
   - 在 Visual Studio 中配置启动程序为 CorelDRAW
   - 附加到进程进行调试

2. **测试**
   - 先在测试文档上测试
   - 检查内存泄漏
   - 验证所有功能

---

## 💡 快速代码参考

### 获取 CorelDRAW 应用程序对象

```csharp
Application corelApp = (Application)application;
```

### 获取选中的形状

```csharp
Selection selection = corelApp.ActiveDocument.Selection;
foreach (Shape shape in selection)
{
    // 处理形状
    string name = shape.Name;
}
```

### 创建矩形

```csharp
Shape rectangle = corelApp.ActiveDocument.ActiveLayer.CreateRectangle(x1, y1, x2, y2);
```

### 创建文字

```csharp
Shape textShape = corelApp.ActiveDocument.ActiveLayer.CreateArtisticText(x, y, "Hello World");
textShape.Font.Name = "Arial";
textShape.Font.Size = 24;
```

### 导出图形

```csharp
shape.Export("C:\\output.png", cdrFilter.cdrPNG, cdrExportRange.cdrCurrentPage);
```

### 修改颜色

```csharp
// 填充色
shape.Fill.UniformColor.RGBAssign(255, 0, 0, 255); // 红色

// 轮廓色
shape.Outline.Color.RGBAssign(0, 0, 255, 255); // 蓝色
shape.Outline.Width = 2.0;
```

### 对齐操作

```csharp
selection.AlignShapes(
    cdrAlignType.cdrAlignLeft, 
    cdrAlignShapesTo.cdrAlignShapesToPage,
    cdrAlignType.cdrNoAlignment);
```

---

## 🔧 下一步开发建议

### 短期目标
- [ ] 完善现有功能
- [ ] 添加更多形状工具
- [ ] 实现批量导出的完整功能
- [ ] 添加用户设置持久化

### 中期目标
- [ ] 集成 AI API
- [ ] 实现用户登录系统
- [ ] 添加云端数据同步
- [ ] 实现支付模块

### 长期目标
- [ ] 扩展到 50+ 功能
- [ ] 支持多个 CorelDRAW 版本
- [ ] 开发插件市场
- [ ] 完善文档和教程

---

## 📖 学习资源

### 官方文档
- CorelDRAW VSTA 帮助文档
- CorelDRAW SDK 参考
- Microsoft .NET 文档

### 社区资源
- CorelDRAW 官方论坛
- VSTA 开发者社区
- GitHub 开源项目

### 推荐书籍
- CorelDRAW VBA 编程书籍
- .NET WPF 开发相关书籍

---

## ⚠️ 常见问题

### Q: 找不到 Corel.Interop.VGCore.dll
A: 该文件通常在 CorelDRAW 安装目录的 Programs64 文件夹中。

### Q: 如何注册插件
A: 参考 CorelDRAW VSTA 文档，将生成的 DLL 注册到 CorelDRAW 中。

### Q: 如何调试
A: 在 Visual Studio 中配置 CorelDRAW 为启动程序，然后附加到进程调试。

---

祝你开发顺利！🎉