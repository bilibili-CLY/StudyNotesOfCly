namespace DDDDemo.Infrastructure;

using DDDDemo.Domain;

/// <summary>基础设施层：内存仓储实现。领域层不知道数据库，这里负责存取细节。</summary>
public class InMemoryInboundOrderRepository : IInboundOrderRepository
{
    private readonly Dictionary<string, InboundOrder> _db = new();

    public InboundOrder? Get(string orderNo) => _db.GetValueOrDefault(orderNo);

    public void Save(InboundOrder order) => _db[order.OrderNo] = order;
}