namespace DDDDemo.Domain;

/// <summary>领域模型声明的仓储接口：存取聚合根。具体实现放基础设施层，实现依赖倒置。</summary>
public interface IInboundOrderRepository
{
    InboundOrder? Get(string orderNo);
    void Save(InboundOrder order);
}