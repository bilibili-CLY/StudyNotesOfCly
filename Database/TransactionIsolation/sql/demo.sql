-- =====================================================================
-- SQL Server 事务与隔离级别演示
-- 配套笔记：../TransactionIsolation.md
--
-- 说明：本脚本按"场景"分段，多数场景需要开【两个查询窗口】配合，
--      下文用「窗口A」「窗口B」标注各自粘贴执行的代码。
--      建议先用一次临时库：CREATE DATABASE DemoWms;
-- =====================================================================

USE DemoWms;
GO

-- ---------------------------------------------------------------------
-- 0. 建表 + 造数（在任一窗口执行一次即可）
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Inventory', 'U') IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.InventoryFlow', 'U') IS NOT NULL DROP TABLE dbo.InventoryFlow;
GO

CREATE TABLE dbo.Inventory (
    SkuId   INT          NOT NULL PRIMARY KEY,
    SkuName NVARCHAR(50) NOT NULL,
    Qty     INT          NOT NULL
);

CREATE TABLE dbo.InventoryFlow (
    Id      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SkuId   INT NOT NULL,
    Qty     INT NOT NULL,              -- 正数入库 / 负数出库
    BizType NVARCHAR(20) NOT NULL      -- INBOUND / OUTBOUND
);

INSERT INTO dbo.Inventory VALUES (1001, N'白酒礼盒', 100);
INSERT INTO dbo.Inventory VALUES (1002, N'啤酒箱装', 200);
GO

SELECT * FROM dbo.Inventory;
GO

-- ---------------------------------------------------------------------
-- 1. 场景A：事务 + 行锁（写写排队）
--    目标：理解 UPDATE 自动加 X 锁直到提交，后到的写必须等待。
-- ---------------------------------------------------------------------
-- 「窗口A」执行（不提交，故意卡住）：
BEGIN TRAN;
    UPDATE dbo.Inventory SET Qty = Qty - 10 WHERE SkuId = 1001;
    -- 此时窗口A持有 SKU-1001 的排它锁
    SELECT @@ROWCOUNT AS UpdatedRows;
-- 先别 COMMIT，去「窗口B」执行下面这句：

-- 「窗口B」执行（会发现一直转圈 = 被阻塞等待窗口A的锁）：
UPDATE dbo.Inventory SET Qty = Qty - 5 WHERE SkuId = 1001;
-- 想看阻塞来源，另开窗口C查：
-- SELECT request_session_id, blocking_session_id, resource_type, resource_description
--   FROM sys.dm_exec_requests WHERE blocking_session_id > 0;

-- 回到「窗口A」提交，窗口B立即完成：
-- COMMIT;

-- 校验：
SELECT * FROM dbo.Inventory;   -- 1001 = 100 - 10 - 5 = 85
GO

-- ---------------------------------------------------------------------
-- 2. 场景B：脏读（READ UNCOMMITTED 读到未提交数据）
--    目标：理解默认 READ COMMITTED 避免脏读，READ UNCOMMITTED 会脏读。
-- ---------------------------------------------------------------------
-- 「窗口A」执行（不提交）：
BEGIN TRAN;
    UPDATE dbo.Inventory SET Qty = Qty - 10 WHERE SkuId = 1002;
-- 先别提交。

-- 「窗口B」执行 ①（默认隔离级别，读已提交）——会阻塞等待，看不到脏数据：
SELECT Qty FROM dbo.Inventory WHERE SkuId = 1002;

-- 「窗口B」执行 ②（改成读未提交）——立刻读到窗口A未提交的 190：
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SELECT Qty FROM dbo.Inventory WHERE SkuId = 1002;
-- 这一行的值 = 200 - 10 = 190，但窗口A还没提交（甚至可能回滚），所以是脏读。
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;   -- 改回默认

-- 「窗口A」回滚，验证刚才读的 190 是假数据：
-- ROLLBACK;
SELECT * FROM dbo.Inventory;   -- 1002 仍为 200
GO

-- ---------------------------------------------------------------------
-- 3. 场景C：不可重复读（同一事务两次读不一致）
--    目标：READ COMMITTED 的 S 锁"读完即放"，事务内第二次读会看到新值。
-- ---------------------------------------------------------------------
-- 「窗口A」执行（不提交，事务内先读一次）：
BEGIN TRAN;
    SELECT Qty FROM dbo.Inventory WHERE SkuId = 1001;   -- 第一次读

-- 「窗口B」执行（提交一个修改）：
UPDATE dbo.Inventory SET Qty = Qty + 30 WHERE SkuId = 1001;
COMMIT;

-- 「窗口A」再读一次——值变了（不可重复读发生）：
    SELECT Qty FROM dbo.Inventory WHERE SkuId = 1001;   -- 第二次读，值不同
COMMIT;
GO

-- ---------------------------------------------------------------------
-- 4. 场景D：可重复读 / 串行化（看锁行为差异）
--    目标：REPEATABLE READ 把 S 锁保持到事务结束，窗口B被阻塞。
-- ---------------------------------------------------------------------
-- 「窗口A」执行：
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
BEGIN TRAN;
    SELECT Qty FROM dbo.Inventory WHERE SkuId = 1001;   -- 持有 S 锁直到 COMMIT

-- 「窗口B」执行——会被阻塞（S 锁未释放，X 锁进不来）：
UPDATE dbo.Inventory SET Qty = Qty + 10 WHERE SkuId = 1001;

-- 「窗口A」提交后窗口B恢复：
-- COMMIT;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

-- ---------------------------------------------------------------------
-- 5. 场景E：扣库存防超卖（条件更新 + 行锁，WMS 出库过账）
--    目标：并发扣减也不超卖，库存不足时影响行数为 0 自动回滚。
-- ---------------------------------------------------------------------
-- 先把库存复位成已知值，保证演示结果确定：
UPDATE dbo.Inventory SET Qty = 100 WHERE SkuId = 1001;
DELETE FROM dbo.InventoryFlow;
GO

-- 同时开两个窗口，各自执行下面这段"同一份"代码，模拟两个订单并发扣同一个 SKU：
BEGIN TRAN;
    DECLARE @skuId INT = 1001, @qty INT = 60;   -- 两个窗口可分别用 60 / 60 演示排队

    UPDATE dbo.Inventory
       SET Qty = Qty - @qty
     WHERE SkuId = @skuId
       AND Qty >= @qty;                          -- 条件防超卖

    IF @@ROWCOUNT = 0
    BEGIN
        PRINT N'库存不足，事务回滚';
        ROLLBACK;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.InventoryFlow(SkuId, Qty, BizType)
        VALUES (@skuId, -@qty, N'OUTBOUND');
        COMMIT;
        PRINT N'扣减成功';
    END;
GO

-- 校验：并发两个 60 都"成功"时，库存 100 - 60 - 60 = -20？不会！
--   第一个 60 先提交后库存剩 40，第二个 60 的 WHERE Qty >= 60 不成立，被回滚。
SELECT * FROM dbo.Inventory;
SELECT * FROM dbo.InventoryFlow;
GO
