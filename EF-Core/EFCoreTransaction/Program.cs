using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();   // 内存库必须保持打开，连接一关数据库就没了

var options = new DbContextOptionsBuilder<WmsContext>()
    .UseSqlite(connection)
    .Options;

await using var ctx = new WmsContext(options);
await ctx.Database.EnsureCreatedAsync();

await Demo.AllInOneTransactionAsync(ctx);
await Demo.RollbackOnFailureAsync(ctx);

public static class Demo
{
    public static async Task AllInOneTransactionAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景1：显式事务，多步 SQL 一起提交 ==");
        await using var tx = await ctx.Database.BeginTransactionAsync();
        try
        {
            ctx.Inventorys.Add(new Inventory { SkuId = "SKU-1001", Qty = 10 });
            await ctx.SaveChangesAsync();                                    // 纳入事务
            await ctx.Database.ExecuteSqlRawAsync(                            // 纳入事务
                "UPDATE Inventory SET Qty = Qty + 1 WHERE SkuId = {0}", "SKU-1001");

            await tx.CommitAsync();
            var qty = await ctx.Inventorys.AsNoTracking().SingleAsync(i => i.SkuId == "SKU-1001");
            Console.WriteLine($"提交后 SKU-1001 库存 = {qty.Qty}");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public static async Task RollbackOnFailureAsync(WmsContext ctx)
    {
        Console.WriteLine("== 场景2：第二步抛异常，整体回滚 ==");
        await using var tx = await ctx.Database.BeginTransactionAsync();
        try
        {
            ctx.Inventorys.Add(new Inventory { SkuId = "SKU-1002", Qty = 5 });
            await ctx.SaveChangesAsync();                                     // 成功但未提交
            throw new InvalidOperationException("记流水失败，模拟中途出错");

            // await tx.CommitAsync();   // 永远执行不到，改为回滚
        }
        catch
        {
            await tx.RollbackAsync();                                         // 上面的 INSERT 一起撤销
            var count = await ctx.Inventorys.CountAsync(i => i.SkuId == "SKU-1002");
            Console.WriteLine($"回滚后 SKU-1002 行数 = {count}（应为 0，证明整段操作被撤销）");
        }
    }
}

[Table("Inventory")]
public class Inventory
{
    public int Id { get; set; }
    public string SkuId { get; set; } = "";
    public int Qty { get; set; }
}

public class WmsContext(DbContextOptions<WmsContext> options) : DbContext(options)
{
    public DbSet<Inventory> Inventorys => Set<Inventory>();
}