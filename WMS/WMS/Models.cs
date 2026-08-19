namespace WmsDemo;

public enum StockStatus { PendingPutaway, Available, Frozen }

/// <summary>库存台账行：同一个 SKU + 批次 + 库位，就是一条库存。</summary>
public class StockItem
{
    public string Sku { get; init; } = "";
    public string Batch { get; init; } = "";
    public string BinCode { get; set; } = "";
    public int Qty { get; set; }
    public StockStatus Status { get; set; }

    public override string ToString() => $"{Sku} 批次{Batch} @ {BinCode} = {Qty}（{Status}）";
}

public class OutboundLine
{
    public string Sku { get; init; } = "";
    public int Qty { get; init; }
}

public class OutboundOrder
{
    public string OrderNo { get; init; } = "";
    public string Customer { get; init; } = "";
    public List<OutboundLine> Lines { get; } = new();
}