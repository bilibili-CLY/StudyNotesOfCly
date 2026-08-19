namespace DDDDemo.Domain;

/// <summary>实体：库存台账行。有唯一身份（Sku+库位），库存数量可变，封装增减校验。</summary>
public class StockItem
{
    public string Sku { get; }
    public string BinCode { get; }
    public int Qty { get; private set; }

    public StockItem(string sku, string binCode, int qty)
    {
        Sku = sku;
        BinCode = binCode;
        Qty = qty;
    }

    public void In(Quantity q) => Qty += q.Value;

    public bool TryOut(Quantity q)
    {
        if (q.Value > Qty) return false;
        Qty -= q.Value;
        return true;
    }

    public override string ToString() => $"{Sku}@{BinCode}: {Qty}";
}
