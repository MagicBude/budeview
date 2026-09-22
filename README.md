# BudeView

A lightweight, local-first, Windows-first desktop image viewer.

BudeView 的目标是：

`双击图片 → 尽快看到 → 顺滑切图 → 自然缩放/拖动 → 关闭`

而不是构建图库数据库、账号系统、云相册或图片编辑器。

## 当前版本

**V0.3 — Navigation & Performance**

当前支持：

- JPEG / PNG
- 文件打开 / 启动参数打开
- 同目录 Natural Sort
- ← / → 前后切图
- 邻图 Preload ±2
- 有界自适应 Decode Cache
- Fit / Fit Width / Fit Height / Fill
- 100% Pixel
- Zoom-to-cursor
- Pan
- F11 Fullscreen
- JSONL Trace
- Native AOT Startup Benchmark

## 技术栈

- .NET 10 LTS
- Avalonia 12
- Native AOT
- Windows-first

Avalonia 负责 GUI；Decoder / Cache / Color / Huge Image / Heavy Codec 继续保持独立架构边界。

## 开发检查

```powershell
.\scripts\check.ps1
```

## 运行

```powershell
.\scripts\run.ps1 -Image "C:\path\to\image.jpg"
```

## Trace

```powershell
.\scripts\run-trace.ps1 -Image "C:\path\to\image.jpg"
```

## Startup Benchmark

```powershell
.\scripts\benchmark-startup.ps1 `
  -Image "C:\path\to\image.jpg" `
  -Runs 5
```

详细设计见 `docs/INDEX.md`。
