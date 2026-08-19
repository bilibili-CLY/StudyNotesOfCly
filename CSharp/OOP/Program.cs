namespace OopDemo;

static class Program
{
    static void Main()
    {
        Console.WriteLine("======== 演示 1：封装 —— 库存台账防呆校验 ========");
        StockItem sku = new("SKU-1001");
        sku.In(100);                                          // 收货入库 100
        sku.In(50);                                           // 再入库 50
        Console.WriteLine($"{sku.Sku} 当前库存 = {sku.Qty}");  // 150
        Console.WriteLine($"尝试出库 200（超库存）：{sku.TryOut(200)}"); // False，拒绝
        Console.WriteLine($"尝试出库 -5（非法数量）：{sku.TryOut(-5)}"); // False，拒绝
        sku.TryOut(30);
        Console.WriteLine($"出库 30 后库存 = {sku.Qty}");       // 120
        // sku.Qty = -100;  // ❌ 编译错误：setter 是 private，外部改不了库存
        // sku.In(-50);     // ❌ 运行报错：In 方法内部校验数量必须大于 0

        Console.WriteLine("\n======== 演示 2：继承 —— 单据公共部分收敛到基类 ========");
        InboundOrder inbound = new("PO-20260819-001");
        OutboundOrder outbound = new("SO-20260819-001");
        inbound.PrintNo();   // 打印单号，方法来自基类（继承复用）
        outbound.PrintNo();
        Console.WriteLine($"入库单初始状态：{inbound.Status}"); // 来自基类，子类自动拥有

        Console.WriteLine("\n======== 演示 3：重载（编译时多态）—— 拣货任务分配 ========");
        PickService pick = new();
        pick.Assign("SKU-1001", 20);                          // 不指定库位：按默认规则
        pick.Assign("SKU-1001", 20, "A-01-02");               // 指定库位：精确定位

        Console.WriteLine("\n======== 演示 4：重写（运行时多态）—— 单据统一过账 ========");
        // 上层只认识基类，不关心具体是哪种子类
        BaseOrder[] orders = { inbound, outbound };
        foreach (BaseOrder o in orders)
        {
            o.Post();   // 运行期各自执行自己的过账逻辑
            Console.WriteLine($"   {o.OrderNo} -> {o.Status}");
        }
        Console.WriteLine("新增单据类型（如调拨单）时，上面的循环一行都不用改 —— 多态的价值");
    }
}

// ---------- 演示 1：封装 ----------
class StockItem(string sku)
{
    private int _qty;                        // 私有字段：外部碰不到
    public string Sku { get; } = sku;
    public int Qty => _qty;                  // 只读对外

    public void In(int n)                    // 入库：带校验
    {
        if (n <= 0) throw new ArgumentException("入库数量必须大于 0");
        _qty += n;
    }

    public bool TryOut(int n)                // 出库：防负数、防超库存
    {
        if (n <= 0 || n > _qty) return false;
        _qty -= n;
        return true;
    }
}

// ---------- 演示 2：继承 ----------
abstract class BaseOrder(string orderNo)
{
    public string OrderNo { get; } = orderNo;
    public string Status { get; protected set; } = "已创建"; // protected：子类可改，外部只读

    public void PrintNo() => Console.WriteLine($"单据号：{OrderNo}"); // 公共方法，子类继承

    public abstract void Post();             // 抽象方法：过账逻辑交给子类
}

class InboundOrder(string orderNo) : BaseOrder(orderNo) // 采购入库单
{
    public override void Post()
    {
        Status = "已过账";
        Console.WriteLine($"[入库单 {OrderNo}] 过账：库存增加");
    }
}

class OutboundOrder(string orderNo) : BaseOrder(orderNo) // 销售出库单
{
    public override void Post()
    {
        Status = "已过账";
        Console.WriteLine($"[出库单 {OrderNo}] 过账：校验库存并扣减");
    }
}

// ---------- 演示 3：重载（编译时多态） ----------
class PickService
{
    public void Assign(string sku, int qty)          // 按默认规则分配库位
    {
        Console.WriteLine($"分配拣货任务：{sku} x {qty}（默认库位规则）");
    }

    public void Assign(string sku, int qty, string bin) // 指定库位
    {
        Console.WriteLine($"分配拣货任务：{sku} x {qty} -> 指定库位 {bin}");
    }
}