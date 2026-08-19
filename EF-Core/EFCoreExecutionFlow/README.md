# EFCoreExecutionFlow 示例

演示 EF Core 底层执行流程：LINQ → 表达式树 → 查询翻译 → SQL 生成 → ADO.NET 执行 → 结果物化 → ChangeTracker。通过 `LogTo` 打印真实生成的 SQL，直观看到"延迟执行""参数化""追踪/非追踪""SaveChanges 写管道"四个核心行为。

## 环境依赖

- .NET SDK 10.0+
- NuGet 包：`Microsoft.EntityFrameworkCore.Sqlite`（内存库，无需安装数据库）

## 运行方式

```bash
dotnet run
```

预期输出要点：

```
== 场景1：延迟执行 ==
写了 Where(...) 但还没 ToList：此刻 SQL 尚未生成、数据库未查询
ToList 后拿到 1 条，SQL 日志已在上方打印     ← 上方可见 SELECT ... FROM "InboundOrders" AS "i" WHERE "i"."Status" = '待收货'

== 场景2：翻译出的 SQL（参数化）==
SELECT "i"."Id", "i"."OrderNo", "i"."Qty", "i"."Status"
FROM "InboundOrders" AS "i"
WHERE "i"."Status" = '已收货' AND "i"."Qty" > 100
注意：SQLite 提供程序会把常量内联进 SQL；生产常用的 SQL Server 则显示为 @__status_0 参数

== 场景3：追踪 vs 非追踪 ==
追踪到的实体是否被 ChangeTracker 管理：Unchanged
AsNoTracking 的实体是否被管理：Detached（没被盯住）

== 场景4：SaveChanges 写管道 ==
修改后还没 SaveChanges：Entry 状态 = Modified（待执行）
SaveChanges 后日志上方应出现 UPDATE 语句
```

## 代码结构

- `Program.cs` — 内存 SQLite 库 + 四个场景：
  - 场景1：写 LINQ 不查库（延迟执行），`ToList` 才触发翻译与执行。
  - 场景2：`ToQueryString()` 不执行直接看翻译出的 SQL，值全部参数化。
  - 场景3：对比追踪 / `AsNoTracking` 两种查询在 ChangeTracker 里的状态。
  - 场景4：改追踪实体属性后 `SaveChanges()` 生成 UPDATE。
- 业务背景：WMS 收货入库单（待收货 → 已收货）。

## 相关笔记

- [EF Core 底层执行流程](../EFCoreExecutionFlow.md)