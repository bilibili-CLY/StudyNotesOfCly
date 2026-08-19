namespace DDDDemo.Domain;

/// <summary>领域事件：收货单已过账。表达"领域里发生了值得下游关注的事实"。</summary>
public record InboundPostedEvent(string OrderNo, string Sku, string BinCode, Quantity Qty);