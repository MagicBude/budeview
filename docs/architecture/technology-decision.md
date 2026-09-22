# Technology Decision

## GUI 主技术栈

- .NET 10 LTS
- Avalonia 12
- Native AOT
- Windows-first

## 选择依据

前期比较了 Native Win32 / Direct2D、Qt 6、Rust + Slint、Avalonia + Native AOT。Finalists 为 Slint 与 Avalonia。

Slint 首图中位数略快，但实验使用不同 JPEG Decoder；Avalonia 在 `decode_end -> presented`、生态、UI 维护与整体平衡上更适合正式产品。

## 非决策

这不意味着所有 Decoder 必须纯 C#，也不意味着 Heavy Codec、Huge Image 或 Renderer 必须绑死在 Avalonia。GUI 决策和 Image Engine 决策保持分离。
