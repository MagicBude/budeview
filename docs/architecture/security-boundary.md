# Security Boundary

图片文件是不可信二进制输入。风险包括 Decoder 漏洞、畸形尺寸、overflow、OOM、压缩炸弹和 native crash。

复杂格式保留 Worker 设计：`BudeView.exe -> IPC -> BudeView.CodecWorker.exe`。Worker 需要明确输入输出、Crash 隔离、Timeout，并预留内存限制。V0.2 只保留接口，不急于创建进程。
