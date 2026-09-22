# Coding Conventions

C# 使用现代语言特性但优先可读性；Nullable 开启；异步 API 后缀 `Async`；取消使用 `CancellationToken`；不使用全局可变单例保存 Viewer State；UI 不直接持有具体 Codec。

命名：类型/公共成员 PascalCase，私有字段 `_camelCase`，局部变量 camelCase。

可恢复图片错误不得导致进程退出；重型 I/O/Decode 不在 UI thread；优化前必须测量。
