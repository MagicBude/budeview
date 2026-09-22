# BudeView

> Working name. A lightweight, local-first, Windows-first desktop image viewer.

BudeView 的目标不是做图片管理器、图片编辑器或云相册，而是做一个**随开随用、快速、安静、正确显示图片**的桌面查看器。

当前仓库版本为 **V0.1 Design Baseline**。本版本**不包含产品代码**，只用于冻结产品方向、交互原则、技术架构、质量目标、工程规范和路线图。后续实现应以这些文档为依据；如果实现与设计发生冲突，应先更新设计与决策记录，再改代码。

## 核心定位

- Viewer First：首先是图片查看器。
- Instant Open：双击图片后尽快看到内容。
- Image First：图片优先于工具栏、面板和功能入口。
- Correct Pixels：方向、Alpha、动画、色彩和缩放必须正确。
- Smooth Navigation：连续切图、缩放、拖动要稳定顺滑。
- Local First：默认不联网、不上传、不需要账号。
- No Scope Creep：不逐步膨胀成 DAM、相册、编辑器或媒体中心。

## 已冻结的技术方向

- Windows-first
- .NET 10 LTS
- Avalonia 12
- Native AOT
- Viewer UI 与 Decoder / Cache / Color / Heavy Codec 解耦
- JPEG / PNG 等基础格式优先走轻量快速路径
- HEIC / JXL / RAW / PSD 等重型格式允许通过独立 Worker / Native Library 接入
- 超大图片采用独立 Huge Image / Tile 路径
- 高风险解码器预留进程隔离边界

详细内容见 [docs/INDEX.md](docs/INDEX.md)。

## V0.1 不包含

- 产品代码
- UI 实现
- 安装包
- 文件关联
- Codec 集成
- Benchmark 程序

这些从 V0.2 实现阶段开始进入。
