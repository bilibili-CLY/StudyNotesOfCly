using System;

namespace DelegateEventDemo;

class Program
{
    static void Main()
    {
        Console.WriteLine("======== 演示 1：自定义委托 —— 出库运费计费规则 ========");
        // 定义委托：按单据算运费。WMS 里同一张单可按不同计费规则收钱。
        CalcFee calc = ByWeight; // 声明时直接把方法放进取（也可先声明后赋值）
        Console.WriteLine($"按重量：10kg x 2 = {calc(10m, 2)} 元");
        calc = ByQuantity; // 换成按数量计费
        Console.WriteLine($"按数量：10件 x 2 = {calc(10m, 2)} 元");

        Console.WriteLine("\n======== 演示 2：委托做参数 —— 库存导入进度回调 ========");
        // 大批量导入库存时，把进度实时回传 UI 进度条
        ImportStock(1000, (done, total) => Console.WriteLine($"  已导入 {done}/{total} 行"));

        Console.WriteLine("\n======== 演示 3：多播委托 —— 出库单过账后的连锁动作 ========");
        Action post = CreatePickTask; // 1. 生成拣货任务
        post += DeductStock; // 2. 扣减库存
        post += WriteLedger; // 3. 写库存台账
        post(); // 按加入顺序依次执行
        post -= DeductStock; // 退订一个，再执行看变化
        Console.WriteLine("退订扣库存后，再执行一次：");
        post();

        Console.WriteLine("\n======== 演示 4：内置 Action / Func + Lambda ========");
        Func<int, int> safetyStock = dailyUsage => dailyUsage * 3; // 安全库存 = 日均用量 x 3
        Action<string> log = msg => Console.WriteLine($"[操作日志] {DateTime.Now:HH:mm:ss} {msg}");
        Console.WriteLine($"安全库存 = {safetyStock(100)} 件");
        log("库存导入完成");

        Console.WriteLine("\n======== 演示 5：事件 —— 库存变动广播 ========");
        InventoryService inv = new();
        inv.StockChanged += (s, e) => Console.WriteLine($"  [审计日志] 扣减 {e.Sku} × {e.Qty}");
        inv.StockChanged += (s, e) =>
            Console.WriteLine($"  [消息队列] 发布 stock.changed: {e.Sku}");
        inv.StockChanged += (s, e) => Console.WriteLine($"  [实时看板] 刷新库存看板");
        inv.Deduct("SKU-1001", 5); // 触发事件，三个订阅者都执行
        inv.Deduct("SKU-1001", 2);
        // inv.StockChanged?.Invoke(this, ...); // ❌ 编译错误：event 不允许在类外触发

        Console.WriteLine("\n======== 演示 6：delegate 与 event 的门禁对比 ========");
        // 同样是"扣库存后通知"，event 有门禁，普通 delegate 没有
        NoGate n = new();
        n.StockChanged += (s, e) => Console.WriteLine($"  [订阅] 收到 {e.Sku} 变动");
        n.Deduct("SKU-2002", 1); // 正常：内部触发
        n.StockChanged?.Invoke(n, new("SKU-9999", 99)); // ✅ delegate 外部可乱触发（无门禁！）
        Console.WriteLine("↑ 看，外部能伪造一次库存变动 —— 这就是 event 存在的意义");
    }

    // ---------- 计费规则 ----------
    static decimal ByWeight(decimal weight, decimal qty) => weight * qty;

    static decimal ByQuantity(decimal quantity, decimal unitPrice) => quantity * unitPrice;

    // ---------- 导入进度（委托做参数） ----------
    static void ImportStock(int rows, Action<int, int>? progress)
    {
        for (int i = 1; i <= rows; i++)
        {
            if (i % 500 == 0)
                progress?.Invoke(i, rows); // 每 500 行回报一次进度
        }
    }

    // ---------- 出库过账连锁动作 ----------
    static void CreatePickTask() => Console.WriteLine("  1. 生成拣货任务");

    static void DeductStock() => Console.WriteLine("  2. 扣减库存");

    static void WriteLedger() => Console.WriteLine("  3. 写库存台账");
}

// 自定义委托类型：入出库计费
delegate decimal CalcFee(decimal factor, decimal qty);

// 库存变动事件参数
class StockChangedEventArgs(string sku, int qty) : EventArgs
{
    public string Sku { get; } = sku;
    public int Qty { get; } = qty;
}

// 演示 5：库存服务 —— event（有门禁）
class InventoryService
{
    // event：外部只能 += / -=，不能直接触发
    public event EventHandler<StockChangedEventArgs>? StockChanged;

    public void Deduct(string sku, int qty)
    {
        Console.WriteLine($"[InventoryService] 扣减库存 {sku} × {qty}");
        StockChanged?.Invoke(this, new StockChangedEventArgs(sku, qty));
    }
}

// 演示 6：无门禁版本 —— 普通 delegate（危险）
class NoGate
{
    // 普通 delegate 字段：外部可直接调用，等于能伪造事件
    public EventHandler<StockChangedEventArgs>? StockChanged;

    public void Deduct(string sku, int qty)
    {
        Console.WriteLine($"[NoGate] 扣减库存 {sku} × {qty}");
        StockChanged?.Invoke(this, new StockChangedEventArgs(sku, qty));
    }
}
