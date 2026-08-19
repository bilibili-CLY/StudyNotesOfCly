namespace DDDDemo.Domain;

/// <summary>聚合内的子实体：入库单明细行。只能由聚合根内部调用方法，外部只读。</summary>
public class InboundLine
{
    public int LineNo { get; }
    public string Sku { get; }
    public Quantity Qty { get; }
    public string BinCode { get; }
    public bool IsPosted { get; private set; }

    public InboundLine(int lineNo, string sku, Quantity qty, string binCode)
    {
        LineNo = lineNo;
        Sku = sku;
        Qty = qty;
        BinCode = binCode;
    }

    internal void MarkPosted() => IsPosted = true;
}
