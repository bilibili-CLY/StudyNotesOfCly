namespace DDDDemo.Domain;

public enum InboundStatus { Created, Received, Posted }

/// <summary>聚合根：入库单。对外是唯一入口，内部保持"单据-明细-状态"的一致性。
/// 业务规则（加行/确认收货/过账）都封装在这里，外部无法直接改明细和状态。</summary>
public class InboundOrder
{
    private readonly List<InboundLine> _lines = new();
    private readonly List<InboundPostedEvent> _events = new();

    public string OrderNo { get; }
    public string Supplier { get; }
    public InboundStatus Status { get; private set; }

    public IReadOnlyList<InboundLine> Lines => _lines;
    public IReadOnlyList<InboundPostedEvent> DomainEvents => _events;

    public InboundOrder(string orderNo, string supplier)
    {
        OrderNo = orderNo;
        Supplier = supplier;
        Status = InboundStatus.Created;
    }

    public void AddLine(int lineNo, string sku, Quantity qty, string binCode)
    {
        if (Status != InboundStatus.Created)
            throw new InvalidOperationException("单据已收货，不能再加行");
        _lines.Add(new InboundLine(lineNo, sku, qty, binCode));
    }

    public void ConfirmReceived()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("没有明细行，无法确认收货");
        Status = InboundStatus.Received;
    }

    public void Post()
    {
        if (Status != InboundStatus.Received)
            throw new InvalidOperationException("只有已收货单据才能过账");
        foreach (var line in _lines.Where(l => !l.IsPosted))
        {
            line.MarkPosted();
            _events.Add(new InboundPostedEvent(OrderNo, line.Sku, line.BinCode, line.Qty));
        }
        Status = InboundStatus.Posted;
    }
}