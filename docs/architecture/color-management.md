# Color Management

P0：EXIF Orientation、sRGB、Embedded ICC 基础读取、Alpha、正确 SDR 输出。

P1：Display P3、多显示器 Profile、HDR、Ultra HDR、10-bit/高位深路径。

Pipeline：Decoded pixels → Source profile → Color transform → Display target → Present。

不能默默丢弃 Profile 后仍宣称“正确显示”。
