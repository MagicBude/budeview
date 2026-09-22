# Project Status

## 当前阶段

**V0.3 — Navigation & Performance**

状态：Windows 实机自动验收与性能基线已通过，准备提交。

## V0.2 已完成

- JPEG / PNG
- 打开图片
- 同目录浏览上下文
- Natural Sort
- Previous / Next
- Fit
- 100% Pixel
- Zoom-to-cursor
- Pan
- Request ID / Cancellation
- 基础有界缓存
- 基础 Trace
- Windows 实机 Build / Self-test / Viewer 验收

## V0.3 已实现

- 邻图 Preload ±2
- 方向感知 Preload 顺序
- 新导航取消旧 Preload
- Preload 不为 speculative cache 驱逐已有有效缓存
- 自适应 Decode Cache Budget：256–512 MiB
- 当前图片全局保护
- Cache Add / Evict / Preload Reject Trace
- Fit Width
- Fit Height
- Fill
- F11 Fullscreen
- Esc 退出 Fullscreen
- Fullscreen 隐藏临时工具栏与状态栏
- Viewer 首帧 Render Trace
- Native AOT Startup Benchmark
- Pointer Capture Lost 清理

## Windows 实机基线

测试图片：8K JPEG。

5 次 Native AOT 启动测试：

- Window P50: 18.3 ms
- Window P95: 64.2 ms
- First Image P50: 357.3 ms
- First Image P95: 419.8 ms
- Decode P50: 333.4 ms
- Decode P95: 366.7 ms
- Peak Working Set P50: 564.6 MiB
- Peak Working Set P95: 564.8 MiB

详细数据见：

`docs/quality/performance-baseline-v0.3.md`

## 下一阶段

**V0.4 — Format Foundation**

计划进入：

- GIF
- WebP
- BMP
- TIFF
- ICO
- SVG
- AVIF
- HEIC
- JPEG XL
- 动画基础

仍需保持统一 Decoder Contract，不允许格式扩展污染 Viewer UI。
