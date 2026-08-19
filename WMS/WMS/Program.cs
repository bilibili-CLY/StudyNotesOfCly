using WmsDemo;

var wh = new WarehouseService();

Console.WriteLine("=========== 第 1 步：收货（入库起点）============");
wh.Receive("SKU-A001", 100, "B2407", "供应商甲");
wh.Receive("SKU-A001", 200, "B2408", "供应商乙");
wh.Receive("SKU-B002", 150, "B2407", "供应商丙");
wh.PrintInventory();

Console.WriteLine("=========== 第 2 步：上架（变为可用库存）============");
wh.Putaway("SKU-A001", "B2407", 100, "A-01-01");
wh.Putaway("SKU-A001", "B2408", 200, "A-01-02");
wh.Putaway("SKU-B002", "B2407", 150, "B-02-01");
wh.PrintInventory();

Console.WriteLine("=========== 第 3 步：接单 → 波次合并 ============");
var so1 = new OutboundOrder
{
    OrderNo = "SO-001", Customer = "客户X",
    Lines = { new() { Sku = "SKU-A001", Qty = 120 } },
};
var so2 = new OutboundOrder
{
    OrderNo = "SO-002", Customer = "客户Y",
    Lines = { new() { Sku = "SKU-A001", Qty = 80 }, new() { Sku = "SKU-B002", Qty = 30 } },
};
var wave = wh.CreateWave(so1, so2);

Console.WriteLine($"=========== 第 4 步：按波次拣货（FIFO 先进先出）============");
foreach (var order in new[] { so1, so2 })
    foreach (var line in order.Lines)
    {
        var picked = wh.PickFifo(line.Sku, line.Qty);
        foreach (var p in picked)
            Console.WriteLine($"  {order.OrderNo} 拣 {line.Sku} x{p.Qty} ← 批次{p.Batch} @ {p.BinCode}");
    }

Console.WriteLine("=========== 第 5 步：复核 → 发运（扣库存 + 回传 ERP）============");
wh.Ship(so1, so2);

Console.WriteLine("=========== 第 6 步：闭环 —— 库存减少触发补货 ============");
Console.WriteLine($"  SKU-A001 剩余可用：{wh.Available("SKU-A001")}（低于安全库存 150，触发补货 → 再走第 1 步收货）");
Console.WriteLine($"  SKU-B002 剩余可用：{wh.Available("SKU-B002")}");

Console.WriteLine("=========== 库存不足会被拦截 ============");
try
{
    wh.PickFifo("SKU-B002", 999);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  {ex.Message}");
}

Console.WriteLine("=========== 最终库存台账 ============");
wh.PrintInventory();