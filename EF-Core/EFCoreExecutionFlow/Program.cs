using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();   // 内存库必须保持连接打开，否则每次操作都是个新库

var options = new DbContextOptionsBuilder<WmsContext>()
    .UseSqlite(connection)
    .LogTo(Console.WriteLine, LogLevel.Information)   // 打印 EF 生成的 SQL，观察管道行为
    .Options;

await using var ctx = new WmsContext(options);
await ctx.Database.EnsureCreatedAsync();

// 预置两条收货入库单
ctx.InboundOrders.AddRange(
    new InboundOrder { OrderNo = "IN-20260819-001", Status = "待收货", Qty = 100 },
    new InboundOrder { OrderNo = "IN-20260819-002", Status = "已收货", Qty = 200 });
await ctx.SaveChangesAsync();
Console.WriteLine("=== 预置数据完成（此时已发生一次 INSERT 事务）===\n");

await Demo.DeferredExecutionAsync(ctx);
await Demo.ShowTranslatedSqlAsync(ctx);
await Demo.TrackingVsNoTrackingAsync(ctx);
await Demo.SaveChangesPipelineAsync(ctx);

public static class Demo
{
    // 场景1：延迟执行 —— 写 LINQ 只是记表达式树，要数据那一刻才翻译并执行
    public static async Task DeferredExecutionAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景1：延迟执行 ==");
        var q = ctx.InboundOrders.Where(o => o.Status == "待收货");

        Console.WriteLine("写了 Where(...) 但还没 ToList：此刻 SQL 尚未生成、数据库未查询");
        Console.WriteLine($"q 的类型：{q.GetType().Name}（IQueryable，只是'菜谱'）\n");

        var list = await q.ToListAsync();   // ★ 触发点：到这里才真正翻译 SQL 并查库
        Console.WriteLine($"ToList 后拿到 {list.Count} 条，SQL 日志已在上方打印\n");
    }

    // 场景2：ToQueryString —— 不执行也能看到翻译出的 SQL，且值是 @参数
    public static async Task ShowTranslatedSqlAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景2：翻译出的 SQL（参数化）==");
        var q = ctx.InboundOrders.Where(o => o.Status == "已收货" && o.Qty > 100);
        Console.WriteLine(q.ToQueryString());   // 只翻译，不执行，不需要 await
        Console.WriteLine("注意值都变成了 @__status_0 / @__qty_1 参数，而不是拼进 SQL\n");
        await Task.CompletedTask;
    }

    // 场景3：追踪 vs 非追踪 —— 谁被 ChangeTracker 盯住
    public static async Task TrackingVsNoTrackingAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景3：追踪 vs 非追踪 ==");
        var tracked = await ctx.InboundOrders.FirstAsync(o => o.OrderNo == "IN-20260819-001");
        var notTracked = await ctx.InboundOrders.AsNoTracking()
            .FirstAsync(o => o.OrderNo == "IN-20260819-002");

        Console.WriteLine($"追踪到的实体是否被 ChangeTracker 管理：{ctx.Entry(tracked).State}");
        Console.WriteLine($"AsNoTracking 的实体是否被管理：{ctx.Entry(notTracked).State}（Detached = 没被盯住）\n");
    }

    // 场景4：保存管道 —— 改追踪到的对象，SaveChanges 时生成 UPDATE
    public static async Task SaveChangesPipelineAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景4：SaveChanges 写管道 ==");
        var order = await ctx.InboundOrders.FirstAsync(o => o.OrderNo == "IN-20260819-001");
        order.Status = "已收货";                     // 改属性，ChangeTracker 通过快照发现变化
        Console.WriteLine($"修改后还没 SaveChanges：Entry 状态 = {ctx.Entry(order).State}（Modified 是'待执行'状态）");

        await ctx.SaveChangesAsync();                // ★ 触发点：把变化翻译成 UPDATE 并执行
        Console.WriteLine("SaveChanges 后日志上方应出现 UPDATE 语句\n");
    }
}

public class InboundOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = "";
    public string Status { get; set; } = "";
    public int Qty { get; set; }
}

public class WmsContext(DbContextOptions<WmsContext> options) : DbContext(options)
{
    public DbSet<InboundOrder> InboundOrders => Set<InboundOrder>();
}