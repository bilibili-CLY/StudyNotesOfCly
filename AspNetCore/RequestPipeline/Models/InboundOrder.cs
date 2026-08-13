namespace RequestPipelineDemo;

/// <summary>
/// 收货入库单（对应 WMS 里"收货上架"环节的单据）。
/// 前端把 JSON 发过来后，由框架的【模型绑定】把它填进这个类的属性。
/// </summary>
public class InboundOrder
{
    /// <summary>入库单号，对应 WMS 里的收货单 ASN 号</summary>
    public string? OrderNo { get; set; }

    /// <summary>物料编码，例如 SKU-1001</summary>
    public string? Sku { get; set; }

    /// <summary>收货数量</summary>
    public int Qty { get; set; }

    /// <summary>入库仓库</summary>
    public string? Warehouse { get; set; }
}
