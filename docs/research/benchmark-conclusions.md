# Technology Benchmark Conclusions

前期一次性技术实验比较 Native Win32/Direct2D、Qt 6、Rust/Slint、.NET/Avalonia。Finalists 为 Slint 与 Avalonia。

最终采用 **.NET 10 LTS + Avalonia 12 + Native AOT**。Slint 显示出良好潜力，但没有形成足以抵消生态和维护成本差异的优势。实验中两条路线使用各自 Decoder，因此纯 Decode 指标不能解释为 GUI 框架性能。

实验仓库完成后不作为正式产品依赖。
