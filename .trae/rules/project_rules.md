# QiTuCDR 项目规则

## Karpathy AI 编程守则 — 4 条原则

### 1. 编码前先思考
- 明确说出你的假设。如果不确定，就问。
- 如果存在多种理解方式，列出来。
- 如果有更简单的方案，说出来。
- 如果有什么不清楚，停下来。说明哪里困惑。

### 2. 简单优先
- 不要加没被要求的功能。
- 单次使用的代码不要搞抽象。
- 不要加没被要求的"灵活性"。
- 不要为不可能的场景写错误处理。
- 如果 200 行能缩成 50 行，就重写。

### 3. 外科手术式修改
- 不要"改进"相邻的代码或格式。
- 不要重构没坏的东西。
- 匹配现有风格，即使你不喜欢。
- 如果发现死代码，提一句——但别删。

### 4. 目标驱动执行
- "加个验证" → "写测试，然后让测试通过"
- "修这个 bug" → "用测试复现，然后修复"
- "重构 X" → "确保重构前后测试都通过"

---

## QiTuCDR 项目专属规则

### 版本隔离
- **仅操作 V26**，路径：`C:\Program Files\Corel\CorelDRAW Graphics Suite\26\Programs64`
- **严禁以任何方式停止 V27 进程**，taskkill 必须按路径过滤仅杀 `26\Programs64` 下的 CorelDRW.exe
- 部署目录：`26\Programs64\Addons\QiTuCDR\`

### WPF/CDR Addon 规范
- XSLT 编码 UTF-8
- AddonEntry 必须自包含 UserControl.Resources（禁用 pack://URI）
- 根级属性禁用 StaticResource（仅根级禁用）
- enable=true
- ViewModel 禁止构造引用外部 Resource 的 View（创建纯 BOOL 属性 ViewModel）
- 修改后必须 F8 刷新
- 禁止 Popup.AllowsTransparency + DropShadowEffect + CornerRadius（黑角 bug）
- XSLT userCaption 纯中文"企图插件"

### 编译
- MSBuild：`C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`
- 解决方案：`plugin\QiTuCDR\QiTuCDR.sln`
- 参数：`/t:Clean,Build /p:Configuration=Debug /p:Platform=x64`
- 输出：`bin\Debug\QiTuCDR.dll`
