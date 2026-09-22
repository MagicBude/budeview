# Image Pipeline

流程：Resolve input → Probe → 建立目录上下文 → 当前图最高优先级解码 → Present → 邻图 Preload → 元数据/色彩异步读取。

优先级：`Current > predicted neighbor > metadata > distant preload`。

快速切图时过期任务应取消；不可中断 native decode 完成后也必须丢弃过期结果。只有当前 Request ID 可以进入 Present。
