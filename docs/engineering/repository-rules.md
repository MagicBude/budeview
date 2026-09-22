# Repository Rules

第一阶段保持单应用，不要过早 Monorepo。实现阶段初始建议 `src/BudeView`、`tests/BudeView.Tests`、`docs`。只有明确出现复用边界时再拆 Core/Codecs/CodecWorker。

文档以中文为主，代码符号和技术名保留英文；不重复新增多个总清单；PROJECT_STATUS 和 CHANGELOG 持续维护。

新增依赖前检查用途、热路径、Native AOT、许可证、安全维护状态和可隔离性。
