# Milestone Exit Criteria

## V0.1

已完成：

- 产品定位
- Out of Scope
- 架构
- 技术栈
- P0/P1/P2
- Roadmap
- 工程规范

## V0.2

已完成：

- JPEG/PNG 可用
- 目录切图
- 100% / Fit / Zoom / Pan
- 过期请求保护
- 基础 Self-test
- 基础 Trace
- Windows 实机验收

## V0.3

完成条件：

- Preload / Cache 有明确预算与取消机制
- 高频导航优先于 Preload
- 已缓存邻图可产生 cache hit
- 内存不会随目录图片数量无限增长
- Fit Width / Height / Fill 行为正确
- Fullscreen 可用
- 首帧 Render 可被 Trace
- Native AOT Startup Benchmark 可复现
- 核心缩放/Pan 体验无回归

## V0.4

完成条件：

- 新格式进入统一 Decoder Contract
- 动画基础正确
- 格式扩展不污染 Viewer UI
- 每个新增格式都有 Corpus 样本
