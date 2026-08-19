namespace WmsDemo;

/// <summary>模拟一个 WMS 核心引擎，覆盖"收货 → 上架 → 波次 → 拣货 → 发运 → 补货"的业务闭环。</summary>
public class WarehouseService
{
    private readonly List<StockItem> _stock = new();
    private readonly Dictionary<string, List<OutboundOrder>> _waves = new();
    private int _waveSeq;

    // ---------- 入库 ----------

    /// <summary>收货：货到先登记为"待上架"，此时还不能拣货出库。</summary>
    public void Receive(string sku, int qty, string batch, string supplier)
    {
        Console.WriteLine($"  收货：{sku} x{qty}，批次{batch}（来源：{supplier}）");
        _stock.Add(new StockItem { Sku = sku, Batch = batch, Qty = qty, Status = StockStatus.PendingPutaway, BinCode = "(待上架)" });
    }

    /// <summary>上架：放到指定库位，库存变"可用"。</summary>
    public void Putaway(string sku, string batch, int qty, string bin)
    {
        var item = _stock.First(s => s.Sku == sku && s.Batch == batch && s.Status == StockStatus.PendingPutaway);
        item.BinCode = bin;
        item.Status = StockStatus.Available;
        Console.WriteLine($"  上架：{sku} 批次{batch} x{qty} → 库位 {bin}，库存变为可用");
    }

    // ---------- 库存 ----------

    public int Available(string sku) =>
        _stock.Where(s => s.Sku == sku && s.Status == StockStatus.Available).Sum(s => s.Qty);

    public void PrintInventory()
    {
        Console.WriteLine("  ── 库存台账 ──");
        foreach (var s in _stock.Where(s => s.Qty > 0)) Console.WriteLine($"    {s}");
    }

    // ---------- 出库 ----------

    /// <summary>波次：把多张出库单合并成一个拣货批次，统一调度。</summary>
    public string CreateWave(params OutboundOrder[] orders)
    {
        var wave = $"WAVE-{++_waveSeq:000}";
        _waves[wave] = orders.ToList();
        Console.WriteLine($"  波次 {wave}：合并 {orders.Length} 张出库单（{string.Join("、", orders.Select(o => o.OrderNo))}）");
        return wave;
    }

    /// <summary>拣货：按批次最早（FIFO/先进先出）扣减可用库存，库存不足直接拦截。</summary>
    public List<StockItem> PickFifo(string sku, int qty)
    {
        var available = _stock
            .Where(s => s.Sku == sku && s.Status == StockStatus.Available && s.Qty > 0)
            .OrderBy(s => s.Batch)          // 批次最小的最先出 = 最早入库的先出
            .ToList();

        int total = available.Sum(s => s.Qty);
        if (total < qty)
            throw new InvalidOperationException($"[拦截] {sku} 库存不足：需要 {qty}，可用 {total}");

        int rest = qty;
        var picked = new List<(StockItem Item, int Qty)>();
        foreach (var item in available)
        {
            if (rest == 0) break;
            int take = Math.Min(item.Qty, rest);
            item.Qty -= take;
            rest -= take;
            picked.Add((item, take));
        }
        return picked.Select(p => new StockItem
        {
            Sku = p.Item.Sku,
            Batch = p.Item.Batch,
            BinCode = p.Item.BinCode,
            Qty = p.Qty,
            Status = StockStatus.Available,
        }).ToList();
    }

    /// <summary>发运：出库完成，打印明细（真实系统这里会过账并回传 ERP）。</summary>
    public void Ship(params OutboundOrder[] orders)
    {
        foreach (var o in orders)
            Console.WriteLine($"  发运 {o.OrderNo}（{o.Customer}）：{string.Join("、", o.Lines.Select(l => $"{l.Sku} x{l.Qty}"))}，扣减库存并回传 ERP");
    }
}