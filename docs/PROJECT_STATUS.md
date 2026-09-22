# Project Status

## 当前阶段

**V0.1 — Design Baseline**

状态：设计冻结中，尚未进入产品代码实现。

## 已完成

- 竞品功能与交互调研
- Viewer 产品边界收敛
- Windows-first 决策
- 技术路线 Benchmark
- .NET 10 + Avalonia 12 + Native AOT 决策
- Viewer Core / Decoder / Cache / Color / Huge Image 分层设计
- P0 / P1 / P2 功能范围
- 性能预算与测试原则
- 工程与仓库规范
- 初版路线图

## 下一阶段

**V0.2 — Viewer Foundation**

只实现最基础、可验证的 Viewer 纵向切片：

- JPEG / PNG
- 打开图片
- 同目录浏览上下文
- Natural Sort
- Previous / Next
- Fit / 100%
- Zoom-to-cursor
- Pan
- 异步 Decode Request
- Cancellation
- 基础有界缓存

不在 V0.2 引入现代格式、HDR、RAW、Archive、Compare 等扩展能力。
