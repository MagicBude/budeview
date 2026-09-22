# Testing Strategy

单测：Natural Sort、File filter、Directory context、Viewer state、Fit/Fill math、Zoom anchor、Cache eviction、Cancellation、Format probing。

集成：Open→Decode→Present、Previous/Next、快速切图、损坏文件、Metadata、Color、Animation。

Corpus 覆盖普通/8K/超大 JPEG、Alpha/超大 PNG、GIF/APNG/WebP、AVIF/HEIC/JXL、SVG/TIFF、PSD/RAW、HDR/EXR、损坏和截断文件。

任何导致 Crash、OOM、Wrong Orientation/Color/Alpha/Animation 的样本都应进入回归 Corpus。
