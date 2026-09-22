# User Scenarios

## A：资源管理器中双击图片
当前图优先显示，目录扫描和邻图预加载不能阻塞首图。

## B：查看 8K 壁纸
默认 Fit，滚轮以鼠标为锚点缩放，左键拖动，一键回 100% / Fit。

## C：开发截图 / UI 素材
100% 必须像素准确，支持高倍率查看和 Alpha 背景切换。

## D：相机照片
正确 Orientation、ICC、EXIF，连续切图不明显白屏。

## E：超大长图
超过阈值后走 Huge Image / Tile 路径，避免不可控内存。

## F：复杂格式
PSD / RAW / HEIC / JXL 等 Decoder 异常不应轻易拖垮整个 Viewer。
