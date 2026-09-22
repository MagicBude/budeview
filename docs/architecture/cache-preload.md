# Cache & Preload

Cache 类型：Probe/Metadata、Decode、Thumbnail、Huge Image Tile。

基础策略：Memory Budget、LRU/cost-aware eviction、单文件上限、大图独立预算、当前图优先于 preload、邻近默认 ±2。

禁止无上限缓存整个目录、禁止 preload 阻塞当前图、禁止旧 preload 覆盖当前请求。
