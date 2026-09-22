# V0.3 Performance Baseline

## 环境

本基线来自 Windows 11 x64 实机上的 Native AOT 发布版本。

测试图片：

`1_DressingRoom2_8k.jpg`

测试次数：

5 次。

## 结果

| Metric | P50 | P95 |
| --- | ---: | ---: |
| Process -> Window | 18.3 ms | 64.2 ms |
| Process -> First Image | 357.3 ms | 419.8 ms |
| Initial Decode | 333.4 ms | 366.7 ms |
| Peak Working Set | 564.6 MiB | 564.8 MiB |

## 单次记录

| Run | Window | First Image | Decode | Peak WS |
| --- | ---: | ---: | ---: | ---: |
| 1 | 64.2 ms | 419.8 ms | 350.1 ms | 564.8 MiB |
| 2 | 22.0 ms | 394.1 ms | 366.7 ms | 564.7 MiB |
| 3 | 18.3 ms | 357.3 ms | 333.4 ms | 564.6 MiB |
| 4 | 16.3 ms | 349.7 ms | 328.3 ms | 564.4 MiB |
| 5 | 15.9 ms | 354.0 ms | 331.3 ms | 563.1 MiB |

## 解读

- 第一轮明显包含更多冷启动成本，因此 Window P95 被 Run 1 拉高。
- First Image 的主要成本仍然是 8K JPEG Decode。
- `Process -> Window` 在热启动后保持在约 16–22 ms。
- Peak Working Set 在 5 次测试中非常稳定，没有出现持续增长。
- 这份数据是 BudeView 自身的 V0.3 回归基线，不与早期技术选型实验仓库直接做严格横向比较。

## 后续用途

V0.4 及以后每次涉及 Decoder、Cache、Renderer 或 Native AOT 配置的较大变更，都应至少用同一测试图片复跑该基线，检查：

- First Image 是否明显退化
- Decode 是否明显退化
- Peak Working Set 是否异常上涨
- Window 创建是否明显变慢
