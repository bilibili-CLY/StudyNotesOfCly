# EFCoreTransaction 示例

演示 EF Core 显式事务 `Database.BeginTransaction()`：让同一上下文内的 `SaveChanges()` 与 `ExecuteSqlRaw()` 所有 SQL 操作共用一个事务。

## 环境依赖

- .NET SDK 10.0+
- NuGet 包：`Microsoft.EntityFrameworkCore.Sqlite`（内存库，无需安装数据库）

## 运行方式

```bash
dotnet run
```

预期输出：

```
== 场景1：显式事务，多步 SQL 一起提交 ==
提交后 SKU-1001 库存 = 11
== 场景2：第二步抛异常，整体回滚 ==
回滚后 SKU-1002 行数 = 0（应为 0，证明整段操作被撤销）
```

## 代码结构

- `Program.cs` — 内存 SQLite 库 + 两个演示场景：
  - 场景1：`Add` + `SaveChanges` + `ExecuteSqlRaw` 全部纳入一个事务后提交。
  - 场景2：事务内第二步抛异常，验证 `RollbackAsync` 把已执行的 `SaveChanges` 一并撤销。
- 业务背景：WMS 收货过账"加库存 + 记流水 + 更单据状态"必须同生共死的场景。