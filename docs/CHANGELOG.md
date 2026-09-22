# Changelog

## V0.3.0 — Navigation & Performance

- 新增方向感知的邻图 Preload ±2。
- 新导航请求会立即取消上一批 Preload。
- Preload 采用机会式缓存策略，不为了 speculative decode 驱逐已有有效缓存。
- Decode Cache 从固定 256 MiB 调整为 256–512 MiB 自适应预算。
- Cache 增加全局当前图片保护，避免后台任务使用过期 protected path。
- 新增 cache budget、cache add、cache evict、preload reject 等 Trace。
- 强化 Decode cancellation，取消后不得泄漏刚完成的 Bitmap。
- 新增 Fit Width、Fit Height 和 Fill。
- 新增 F11 Fullscreen 与 Esc 退出 Fullscreen。
- Fullscreen 下隐藏 V0.3 临时工具栏和状态栏。
- Viewer Surface 新增首帧 Render 事件，用于区分 decode 完成与真正提交绘制。
- 新增 Native AOT 单图启动性能 Benchmark 脚本。
- 增加 `--benchmark-once` 内部诊断参数。
- 补充 PointerCaptureLost 处理，避免异常丢失 Pointer Capture 后残留拖动状态。
- BudeView 自有命令行参数不再交给 Avalonia lifetime 解析。
- 更新 Self-test，覆盖 Benchmark 参数和 Cache Budget Policy。
- 完成 Windows 实机 Build、Self-test 和 Native AOT 发布验证。
- 建立 V0.3 8K JPEG 启动性能基线。
- 修正检查脚本结束提示，使其正确显示 V0.3。

本版本仍只支持 JPEG / PNG，不提前扩展图片格式。
