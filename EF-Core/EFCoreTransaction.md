# EF Core 显式事务（所有 SQL 操作共用一个事务）

- date: 2026-08-19
- tags: [EF Core, 事务, C#, WMS, 库存]
- summary: 让同一个上下文内的 `SaveChanges()`、`ExecuteSqlRaw()` 等所有 SQL 操作都在一个事务里的方法：`context.Database.BeginTransaction()`。默认每次 `SaveChanges()` 自成一个事务，`ExecuteSqlRaw()` 只包它自己那一条。

## 概述 / Overview

> 一句话先说结论：
> **默认情况下，EF Core 每次 `SaveChanges()` 都是单独一个事务，`ExecuteSqlRaw()` 也只自动包它自己那一条。要让一段代码里**所有** SQL 操作要么全成、要么全不成，就用 `context.Database.BeginTransaction()` 开启显式事务，之后的 `SaveChanges()`、`ExecuteSqlRaw()` 会自动纳入这个事务，最后 `Commit()` / `Rollback()`。**

类比仓库账本：收货过账要"加库存 + 记流水 + 改单据状态"三处一起改，在 EF Core 里不显式开事务的话，三处各走各的事务，中途失败就会出现"库存加了但流水没记"。用 `BeginTransaction()` 把它们绑成同一本账，一笔到底。

---

## 核心知识点 / Key Points

### 1. 默认行为（为什么需要显式事务）

| 写法 | 默认事务行为 |
|------|--------------|
| `context.SaveChanges()` | 单独一个事务，内部所有 INSERT/UPDATE/DELETE 同生共死 |
| `context.Database.ExecuteSqlRaw(sql, ...)` | 自动包一个事务，但只包它自己那一条 |
| 多个 `SaveChanges()` / `ExecuteSqlRaw()` 连写 | 各自独立事务，中间失败互不影响 |

> 所以"两个操作必须同生共死"时，默认行为不够用，必须开显式事务。

### 2. 显式事务：`Database.BeginTransaction()`

```csharp
using var tx = await context.Database.BeginTransactionAsync();
try
{
    context.Inventorys.Add(new Inventory { SkuId = "SKU-1001", Qty = 10 });   // 纳入事务
    await context.SaveChangesAsync();                                         // 纳入事务，不再自开事务
    await context.Database.ExecuteSqlRawAsync(                                // 纳入事务
        "UPDATE Inventory SET Qty = Qty + 1 WHERE SkuId = {0}", "SKU-1001");

    await tx.CommitAsync();        // 全部成功 -> 一起提交
}
catch
{
    await tx.RollbackAsync();      // 任一步失败 -> 全部回滚
    throw;
}
```

要点：

- `BeginTransaction()` 拿到的是 `IDbContextTransaction`，事务结束建议 `using` 释放。
- 事务开启后，同一上下文上的 `SaveChanges()` 不再自开新事务，而是**并入**已开启的事务。
- 异步对应 `BeginTransactionAsync()` / `CommitAsync()` / `RollbackAsync()`；同一条连接内所有 SQL 走同一个事务。

### 3. 备选：`TransactionScope`

也可以用 .NET 的 `TransactionScope` 让 EF Core 操作纳入环境事务，但需要 `TransactionScopeAsyncFlowOption.Enabled` 支持异步，管理更隐式。**推荐优先用 `BeginTransaction()`**，事务边界更清晰、可控。

### 4. WMS 业务场景

收货过账（数量加库存 + 写库存流水 + 更新入库单状态）、出库扣减（扣库存 + 记流水 + 更单据状态）这类"多处一起改"的流程，都用显式事务包起来，保证业务一致性。

---

## 代码示例 / Code Example

可运行示例见 [./EFCoreTransaction/](./EFCoreTransaction/)，运行说明见 [./EFCoreTransaction/README.md](./EFCoreTransaction/README.md)。

---

## 面试回答话术 / Interview Q&A

> 每条约 30~60 字，可直接背。先自己默答，再看答案。

**Q1：EF Core 里让所有 SQL 操作都走同一个事务的方法是什么？**
A：用 `context.Database.BeginTransaction()` 开启显式事务，之后的 SaveChanges、ExecuteSqlRaw 自动纳入，最后 Commit/Rollback；默认每次 SaveChanges 自成一个事务。

**Q2：默认情况下 SaveChanges 和 ExecuteSqlRaw 是事务性的吗？**
A：是，但各自独立。SaveChanges 单独一个事务；ExecuteSqlRaw 只包自己那一条；多个操作要同生共死必须显式开事务。

**Q3：BeginTransaction 后多次 SaveChanges 会怎样？**
A：不会各自开新事务，而是并入已开启的事务；任一失败回滚则之前的变更一起回滚，保证整段操作原子性。

**Q4：TransactionScope 和 BeginTransaction 怎么选？**
A：BeginTransaction 边界清晰、自带异步 API，优先推荐；TransactionScope 自动把作用域内连接纳入环境事务，但异步需显式开启配置，管理较隐式。

**Q5：收货过账在 EF Core 里怎么写保证一致性？**
A：BeginTransaction 包住"加库存 + 记流水 + 更单据状态"，三处任一失败整体 Rollback，成功一起 Commit，避免中间状态。

---

## 参考链接 / References

- [微软官方文档 - 在 EF Core 中使用事务](https://learn.microsoft.com/zh-cn/ef/core/saving/transactions)
- [微软官方文档 - 显式事务](https://learn.microsoft.com/zh-cn/ef/core/saving/transactions#controlling-transactions)
- [微软官方文档 - ExecuteSqlRaw 原样执行 SQL](https://learn.microsoft.com/zh-cn/ef/core/querying/sql-queries)

---

## 踩坑记录 / Troubleshooting

| 现象 | 原因 | 解决办法 |
|------|------|----------|
| 忘写 Commit，数据没落库 | 只 Rollback 路径有处理，成功路径漏了 Commit | 成功路径显式 `CommitAsync()`，并用 `using` 保证释放 |
| 多个操作中间失败，部分数据已提交 | 没开显式事务，各自独立提交 | 用 `BeginTransaction()` 包住整段操作 |
| 事务一直开着导致连接/锁泄漏 | `BeginTransaction()` 结果没释放 | `using var tx = await ...BeginTransactionAsync()` |
| 用另一个上下文对象做操作不在同一事务 | 事务绑定在开启它的那个 DbContext 上 | 同一事务内的操作必须在同一个 DbContext 实例上执行 |