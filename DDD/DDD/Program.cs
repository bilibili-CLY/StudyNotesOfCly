using DDDDemo.Application;
using DDDDemo.Domain;
using DDDDemo.Infrastructure;

// ---- 组装（此处模拟 DI 容器注入，真实项目由 Program.cs 统一注册） ----
var repo = new InMemoryInboundOrderRepository();

// 库存台账：模拟"库存上下文"订阅"收货上下文"发布的领域事件
var stock = new Dictionary<string, StockItem>();
void OnPosted(InboundPostedEvent e)
{
    var key = $"{e.Sku}@{e.BinCode}";
    if (!stock.TryGetValue(key, out var item))
    {
        item = new StockItem(e.Sku, e.BinCode, 0);
        stock[key] = item;
    }
    item.In(e.Qty);
    Console.WriteLine($"    [领域事件订阅] 库存台账 => {item}");
}

var svc = new InboundAppService(repo, OnPosted);

// ---- 业务用例：收货入库 ----
var order = svc.CreateOrder("IN-20260819-001", "华强供应链");
order.AddLine(1, "SKU-A001", new Quantity(100), "A-01-01");
order.AddLine(2, "SKU-A001", new Quantity(50), "A-01-02");
order.AddLine(3, "SKU-B002", new Quantity(200), "B-02-01");

Console.WriteLine("=== 收货前 ===");
Console.WriteLine($"单号 {order.OrderNo}，状态 {order.Status}，明细 {order.Lines.Count} 行");

Console.WriteLine("=== 确认收货并过账 ===");
svc.ConfirmAndPost(order.OrderNo);

Console.WriteLine("=== 收货后 ===");
Console.WriteLine($"单号 {order.OrderNo}，状态 {order.Status}");
foreach (var l in order.Lines)
    Console.WriteLine($"  行{l.LineNo}: {l.Sku} ×{l.Qty} → 库位 {l.BinCode}（已过账: {l.IsPosted}）");

Console.WriteLine("=== 违反聚合规则会被拦下 ===");
try
{
    order.AddLine(4, "SKU-C003", new Quantity(10), "C-03-01"); // 已过账，禁止加行
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  已拦截：{ex.Message}");
}