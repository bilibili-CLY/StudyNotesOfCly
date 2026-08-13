# RequestPipeline 演示项目

配套笔记：《ASP.NET Core 请求处理管道（新手入门）》 → 返回 [../RequestPipeline.md](../RequestPipeline.md)

## 依赖环境

- .NET SDK 10 或更高（`dotnet --version` 查看）

## 运行

```bash
dotnet run
```

另开一个终端发请求（默认监听 `http://localhost:5xxx`，以启动日志里的地址为准，或用 `ASPNETCORE_URLS=http://localhost:5210 dotnet run` 指定端口）：

```bash
# 1. 模拟"收货入库"：POST 一个 JSON 单据过去
curl -X POST http://localhost:5210/api/inbound/receive \
  -H 'Content-Type: application/json' \
  -d '{"orderNo":"ASN-20260803-001","sku":"SKU-1001","qty":120,"warehouse":"华东仓"}'

# 2. 看"管道步进清单"：按顺序显示每个环节的执行记录
curl http://localhost:5210/api/inbound/trace

# 3. 故意触发异常，看全局异常中间件兜底
curl http://localhost:5210/api/inbound/boom

# 4. 访问挂了 [Authorize] 的接口：会 401，但"预鉴权中间件"已先跑（看 trace）
#    → 证明"在 [Authorize] 之前做事"的正确姿势是中间件，而不是过滤器
curl http://localhost:5210/api/inbound/secure
curl http://localhost:5210/api/inbound/trace
```

## 管道总览（对应笔记知识点）

```mermaid
flowchart LR
    A[HTTP 请求到达] --> B[中间件① 异常兜底<br/>用 try/catch 包住后面一切]
    B --> C[中间件② 请求日志/计时<br/>记下 URL 和耗时]
    C --> D[中间件③ UseRouting 路由<br/>判断请求交给谁]
    D --> E[中间件④ 预鉴权<br/>在鉴权开始前拦截，可读终结点]
    E --> F[中间件⑤ 认证/授权<br/>[Authorize] 判定在此，未过直接 401]
    F --> G[MapControllers 匹配终结点<br/>进入 MVC 过滤器管道]
    G --> H[授权过滤器 PreAuthFilter<br/>模型绑定前的第一道关卡]
    H --> I[模型绑定<br/>把 JSON 填进 InboundOrder]
    I --> J[过滤器 执行前<br/>ModelState 校验 / 记录日志]
    J --> K[控制器方法执行<br/>StockService 加库存]
    K --> L[过滤器 执行后<br/>拿到返回结果]
    L --> M[格式化结果<br/>序列化成 JSON 返回]
    M --> N[响应原路返回<br/>逐层穿过中间件⑤→①]
    N --> O[浏览器/客户端收到响应]
```

## 演示内容（对应笔记知识点）

| 演示 | 请求 | 对应知识点 |
|------|------|-----------|
| 1 | `POST /api/inbound/receive` | 路由匹配 + 模型绑定 + DI + 过滤器 + 结果序列化 |
| 2 | `GET /api/inbound/trace` | 直接查看请求在管道里的完整步进顺序 |
| 3 | `GET /api/inbound/boom` | 抛异常 → 全局异常中间件兜底，返回友好 JSON |
| 4 | `GET /api/inbound/secure` | 挂 `[Authorize]` 返回 401；trace 显示「预鉴权中间件」已在鉴权前执行（中间件方案） |
| 5 | `GET /api/inbound/trace` | 到 MVC 管道内的请求：`PreAuthFilter` 是第一道授权关卡 |
| 6 | 控制台窗口 | 观察 `==> 请求进入` 与耗时日志 |

## 建议动手改

- 在 `Program.cs` 里再 `app.Use(...)` 一个新中间件，重启后看执行顺序变化
- 把中间件②移到 `UseRouting()` **后面**，看请求日志还打不打印（体会"顺序即行为"）
- 把「预鉴权中间件」从 `UseAuthorization()` **之前**挪到**之后**，再访问 `secure`，看它还出现在 trace 里吗
- 把 `POST /api/inbound/receive` 的 JSON 里 `qty` 改成负数或去掉，看 `[ApiController]` 的自动 400 校验
