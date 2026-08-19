# DDD 代码示例运行说明

## 环境

- .NET SDK 10.0+（本仓库所有示例使用 net10.0）
- 无需额外 NuGet 包，纯 BCL

## 运行

```bash
dotnet run
```

## 演示内容（对应 DDD.md 知识点）

1. **值对象** `Quantity` — `record` 不可变，构造时校验数量 > 0
2. **实体** `StockItem` — 有身份（Sku+库位）、库存数量可变，增减走 `In/TryOut` 校验
3. **聚合根** `InboundOrder` — 单据+明细+状态封装在内部，`AddLine/ConfirmReceived/Post` 都带业务规则，外部无法直接改明细
4. **领域事件** `InboundPostedEvent` — 过账后记录事件，由应用层发布，库存台账订阅并响应
5. **仓储接口** `IInboundOrderRepository`（领域层定义）→ `InMemoryInboundOrderRepository`（基础设施层实现），依赖倒置
6. **应用层** `InboundAppService` — 只编排用例：取聚合 → 调聚合根方法 → 发事件 → 保存
7. **规则拦截演示** — 已过账的单据再 `AddLine` 会被聚合根抛异常拦下

建议动手改一改：把 `ConfirmAndPost` 拆开只过账不收货、新增一个值对象 `BinCode`、或把 `StockItem` 移进一个独立的"库存上下文"类库。