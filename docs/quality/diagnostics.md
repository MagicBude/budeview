# Diagnostics

早期保留低成本 Trace：process_start、window_created、folder_scan、probe、decode_queued/start/end/cancelled、cache_hit/miss/evict、present_requested、image_presented、preload_queued、color_transform。

未来开发参数可提供 `--trace-startup`、`--trace-decode`、`--trace-cache`、`--trace-file`。默认用户模式不输出大量日志。
