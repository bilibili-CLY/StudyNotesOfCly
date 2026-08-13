namespace RequestPipelineDemo;

/// <summary>
/// 库存服务：模拟"收货入库后加库存、写流水"。
/// 由依赖注入容器（DI）自动创建并送进控制器构造函数。
/// </summary>
public class StockService
{
    public string AddStock(InboundOrder order)
    {
        // 真实项目里这里会查库存表、做并发控制、写库存流水，
        // 这里只返回一句话代表"入库过账成功"。
        return $"物料 {order.Sku} @ {order.Warehouse} 入库 +{order.Qty}，生成入库流水 {order.OrderNo}";
    }
}
