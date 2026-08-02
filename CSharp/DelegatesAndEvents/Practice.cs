namespace DelegateEventDemo;

public class Practice
{
    InventoryService _inventoryService = new();

    public void Run()
    {
        _inventoryService.StockChange += (s,e) => Console.WriteLine("告诉ERP");
        
        _inventoryService.AddStock(100);
    }
}

class Inventory
{
    static int stock = 100;
}

partial class InventoryService
{
    public event EventHandler StockChange;

    public void AddStock(int stock)
    {
        Console.WriteLine("添加："+stock);
        StockChange?.Invoke(this, EventArgs.Empty);
    }
}