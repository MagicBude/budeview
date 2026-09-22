# Changelog

## V0.2.0 — Viewer Foundation

- 初始化正式 .NET 10 + Avalonia 12.1.2 应用工程。
- 配置 Native AOT-ready、trimming 与 compiled binding 基线。
- 新增 JPEG / PNG 打开能力。
- 新增 Avalonia 12 `IStorageProvider` 文件选择器。
- 支持通过启动参数直接打开图片。
- 当前图片加载与目录扫描并行，避免目录扫描阻塞首图。
- 新增同目录图片上下文与 Natural Sort。
- 新增 Previous / Next 浏览。
- 新增 Fit 与考虑 Windows RenderScaling 的 100% Pixel 模式。
- 新增鼠标位置锚定的连续缩放。
- 新增左键拖动 Pan。
- 新增 Request ID 与 Cancellation，过期请求不得覆盖当前图片。
- 新增 256 MiB 有界 LRU Decode Cache。
- 新增可选 JSONL Trace。
- 新增无额外测试框架依赖的 `--self-test` 基础自检。
- 新增检查、运行、Trace 与 win-x64 Native AOT 发布脚本。
- 修复应用启动文件缺少 `BudeView.Core` 命名空间导入导致的构建失败。
- 修复检查脚本未正确传播外部命令退出码、失败后仍继续并误报通过的问题。

本版本不实现 Preload、现代格式、HDR、Shell 集成或扩展 Viewer 功能。
