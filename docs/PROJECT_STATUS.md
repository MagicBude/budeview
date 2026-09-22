# Project Status

## 当前阶段

**V0.2 — Viewer Foundation**

状态：首个实现候选已建立，等待 Windows 实机编译与交互验收。

## V0.1 已完成

- 产品定位与边界
- 交互设计基线
- 技术架构
- 技术栈决策
- 质量目标
- 工程规范
- Roadmap / Exit Criteria

## V0.2 当前实现范围

- .NET 10 + Avalonia 12.1.2
- Native AOT-ready
- JPEG / PNG
- 文件选择器打开
- 命令行路径打开
- 当前图片优先解码
- 同目录异步扫描
- Natural Sort
- Previous / Next
- Fit
- 100% Pixel（考虑 Windows RenderScaling）
- Zoom-to-cursor
- Pan
- Request ID / Cancellation
- 256 MiB 有界 LRU Decode Cache
- 可选 JSONL Trace
- 最小自检脚本

## V0.2 明确未实现

- Preload（V0.3）
- Fit Width / Height / Fill（V0.3）
- Fullscreen / Frameless（V0.3）
- GIF / WebP / AVIF / HEIC / JXL（V0.4）
- 文件关联与 Shell 操作（V0.5）
- ICC / HDR（V0.6）
- Filmstrip / Archive / RAW / Huge Image（V0.7）

## 当前 Exit Gate

Windows 实机需要通过：

1. `.\scripts\check.ps1`
2. 可打开 JPEG / PNG
3. ← / → 正确切图
4. Natural Sort 正确
5. `F` 切回 Fit
6. `1` 切到 100%
7. 鼠标滚轮以指针位置缩放
8. 左键拖动 Pan
9. 快速切图不出现旧图覆盖新图
10. Trace 可生成且程序退出后文件完整
