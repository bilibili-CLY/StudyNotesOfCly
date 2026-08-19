# TransactionIsolation 演示脚本

配套笔记：《SQL Server 事务与隔离级别（新手入门）》 → 返回 [../TransactionIsolation.md](../TransactionIsolation.md)

## 依赖环境

- SQL Server（任一版本，或 SQL Server on Docker）
- 可用 SSMS / Azure Data Studio，或命令行 `sqlcmd`

用 Docker 起一个临时实例（无本机 SQL Server 时）：

```bash
docker run -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=Your_Strong!Passw0rd" \
  -p 1433:1433 -d --name sqlsrv mcr.microsoft.com/mssql/server:2022-latest
```

## 运行

先建演示库，并整段跑一遍初始化（`demo.sql` 第 0 节）确认环境正常：

```bash
sqlcmd -S localhost -U sa -P 'Your_Strong!Passw0rd' -d master -Q "CREATE DATABASE DemoWms;"
sqlcmd -S localhost -U sa -P 'Your_Strong!Passw0rd' -d DemoWms -i sql/demo.sql
```

> 脚本里标了「窗口A / 窗口B」的场景需要**开两个查询窗口**、分别粘贴对应代码块，才能演示锁等待、脏读、不可重复读等并发现象，单跑一遍看不出效果。

## 场景清单（对应笔记知识点）

| 场景 | 演示内容 | 对应笔记知识点 |
|------|----------|----------------|
| 0 建表造数 | 建 Inventory / InventoryFlow 表并插数据 | - |
| 1 事务+行锁 | 窗口A 更新不提交，窗口B 同航更新被阻塞 | UPDATE 自动加 X 锁、持锁到提交 |
| 2 脏读 | READ UNCOMMITTED 读到未提交数据 | 默认 READ COMMITTED 避免脏读 |
| 3 不可重复读 | 同事务两次读，值被其他提交事务改掉 | S 锁"读完即放"，语句级隔离 |
| 4 可重复读 | S 锁保持到事务结束，窗口B 写被阻塞 | REPEATABLE READ 的锁行为 |
| 5 扣库存防超卖 | 两个窗口并发扣同一 SKU，条件更新自动回滚 | WHERE 库存>=扣减量 + X 行锁 |

## 建议动手改

- 场景 2 里把窗口B 隔离级别改回 `READ COMMITTED`，对比"等待"和"脏读"两种表现
- 场景 4 改成 `SERIALIZABLE` 再跑，体会范围锁带来的更多阻塞
- 场景 5 把两个窗口的 `@qty` 改成 `40 / 40`，观察两个都成功；改成 `60 / 60` 观察第二个被回滚
- 在场景 1 卡住时用 `sys.dm_exec_requests` 查阻塞来源（脚本里有注释示例）
