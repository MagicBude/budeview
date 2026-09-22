# AGENTS.md

本仓库当前是 BudeView 的正式产品仓库。

## 当前阶段

V0.1 = Design Baseline。

在 V0.1 阶段：

- 不新增产品代码。
- 不提前创建复杂工程结构。
- 不为“以后可能用到”提前引入依赖。
- 所有实现决策必须能追溯到 docs 中的产品、架构或决策文档。

## 实现原则

1. Viewer First。
2. Windows-first，不为未确认的跨平台目标提前付出复杂度。
3. 不引入账号、云同步、AI、图库数据库、标签管理、复杂编辑、视频播放。
4. UI 必须服务于图片，不以功能数量为目标。
5. 性能、正确性和可维护性优先于“框架炫技”。
6. Decoder、Cache、Color Pipeline 与 UI 解耦。
7. 对复杂或不可信图片格式保留进程隔离能力。
8. 大图不能默认一次性全量解码到无限内存。
9. 所有缓存必须有明确预算、淘汰策略与取消机制。
10. 需求变更先修改设计文档，再进入实现。

## 文档维护

- `docs/PROJECT_STATUS.md`：当前状态。
- `docs/CHANGELOG.md`：文档和产品版本变化。
- `docs/decisions/decision-log.md`：重要决策。
- `docs/roadmap/roadmap.md`：版本路线。
- `docs/product/feature-scope.md`：功能边界。
- `docs/architecture/architecture-overview.md`：整体架构。

不要新增多个重复的总清单或 `_MANIFEST.md`。
