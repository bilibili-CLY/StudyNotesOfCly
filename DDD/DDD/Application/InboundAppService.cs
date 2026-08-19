namespace DDDDemo.Application;

using DDDDemo.Domain;

/// <summary>应用层：只做用例编排，不写业务规则。
/// 流程 = 取聚合 → 调聚合根方法 → 发布领域事件 → 保存。</summary>
public class InboundAppService
{
    private readonly IInboundOrderRepository _repo;
    private readonly Action<InboundPostedEvent> _onPosted;

    public InboundAppService(IInboundOrderRepository repo, Action<InboundPostedEvent> onPosted)
    {
        _repo = repo;
        _onPosted = onPosted;
    }

    public InboundOrder CreateOrder(string orderNo, string supplier)
    {
        var order = new InboundOrder(orderNo, supplier);
        _repo.Save(order);
        return order;
    }

    public InboundOrder ConfirmAndPost(string orderNo)
    {
        var order = _repo.Get(orderNo) ?? throw new InvalidOperationException("单据不存在");
        order.ConfirmReceived();
        order.Post();
        foreach (var e in order.DomainEvents) _onPosted(e);
        _repo.Save(order);
        return order;
    }
}