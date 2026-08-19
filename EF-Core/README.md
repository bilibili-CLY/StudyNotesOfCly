# EF-Core 笔记索引

- date: 长期维护
- tags: [EF Core, 学习笔记]

## 笔记列表

- [EF Core 显式事务（所有 SQL 操作共用一个事务）](./EFCoreTransaction.md) — 用 `Database.BeginTransaction()` 让同一上下文内的 `SaveChanges()` / `ExecuteSqlRaw()` 等所有 SQL 走同一个事务
- [EF Core 底层执行流程（查询从代码到数据库的旅程）](./EFCoreExecutionFlow.md) — LINQ → 表达式树 → 查询翻译 → SQL 生成 → ADO.NET → 物化 → ChangeTracker 六大环节，讲透延迟执行与追踪