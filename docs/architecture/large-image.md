# Large Image Strategy

普通尺寸走完整解码和内存 Surface。超过像素数、尺寸或预计内存阈值后进入 Huge Image 路径：bounded preview、viewport tile、独立 tile cache，未来可加入 mip pyramid。

目标是“先看到”，而不是等待整张超大图完全展开。V0.2 只保留接口边界，不做完整 Tile Engine。
