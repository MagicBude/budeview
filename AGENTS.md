# AGENTS.md

BudeView 是 Windows-first、local-first、viewer-first 的轻量图片查看器。

## 当前阶段

V0.3 — Navigation & Performance。

## 当前允许范围

- JPEG / PNG
- 同目录浏览
- Natural Sort
- Fit / Fit Width / Fit Height / Fill / 100%
- Zoom / Pan
- Fullscreen
- Request cancellation
- Preload ±2
- 有界 Decode Cache
- Trace / Benchmark

## 当前禁止提前实现

- GIF / WebP / AVIF / HEIC / JXL
- HDR / ICC 完整实现
- RAW / PSD
- Archive
- Filmstrip
- File Association
- Settings
- Tag / Library / Album
- 账号、云、AI、推荐

## 性能规则

1. Current image 永远高于 preload。
2. 用户输入不得等待 preload。
3. 新导航立即取消旧 preload。
4. Preload 不允许导致 Cache 无界增长。
5. Cache 必须有预算。
6. 优化必须有 Trace 或 Benchmark 依据。
7. 不为了减少少量毫秒引入难以维护的复杂架构。

## 工程规则

- Nullable 开启。
- Native AOT 兼容。
- 不在 UI thread 执行重型 Decode / I/O。
- 不使用反射型 DI 作为核心依赖。
- UI 不直接依赖具体 Codec。
- 先修改设计/状态文档，再扩大版本范围。
