# Architecture Overview

```text
Application / Avalonia UI
        │
Viewer State + Actions
        │
Image Session
        │
Decode Scheduler
   ┌────┼─────────┐
 Fast  Huge      Heavy
 Path  Image     Codec Worker
   │     │          │
   └─────┴──────────┘
        │
Unified Image Surface
        │
Color Pipeline
        │
Renderer / Presenter
```

UI 不直接理解 Codec；Viewer Action 不直接操作解码器；Scheduler 负责优先级、取消和预加载；Cache 必须有预算；Heavy Codec 保留独立边界；Huge Image 独立于普通 full-decode 路径。
