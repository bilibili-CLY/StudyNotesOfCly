using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestPipelineDemo;

namespace RequestPipelineDemo;

/// <summary>
/// 收货入库控制器：演示"路由匹配 + 模型绑定 + 依赖注入 + 返回结果"。
/// [ApiController] 让框架启用 WebAPI 约定（自动 400 校验等）。
/// </summary>
[ApiController]
[Route("api/inbound")]
public class InboundController : ControllerBase
{
    private readonly StockService _stock;

    // 控制器构造函数里传入的服务，由 DI 容器按需注入
    public InboundController(StockService stock) => _stock = stock;

    /// <summary>POST /api/inbound/receive —— 收货入库（从请求体 JSON 读参数）</summary>
    [HttpPost("receive")]
    public IActionResult Receive([FromBody] InboundOrder order)
    {
        // 走到这里时：
        //   ① 路由已把 URL 指到本方法；
        //   ② 模型绑定已把请求体 JSON 填进了 order；
        //   ③ 过滤器已跑完"执行前"。
        var message = _stock.AddStock(order);
        return Ok(new
        {
            order.OrderNo,
            message,
            tips = "这行 JSON 就是 MVC 用 JSON 格式化器自动序列化出来的结果"
        });
    }

    /// <summary>
    /// GET /api/inbound/trace —— 返回"请求管道走到现在的全部步骤"，直观看执行顺序
    /// GET /api/inbound/trace，或每次访问其他接口后再来看它
    /// </summary>
    [HttpGet("trace")]
    public IActionResult Trace([FromServices] PipelineTrace trace)
        => Ok(trace.Steps);

    /// <summary>GET /api/inbound/boom —— 故意抛异常，演示全局异常中间件兜底</summary>
    [HttpGet("boom")]
    public IActionResult Boom() => throw new InvalidOperationException("模拟的未处理异常！");

    /// <summary>
    /// GET /api/inbound/secure —— 挂了 [Authorize] 的接口。
    /// 未配置真实登录时它必然返回 401；但访问后去看 trace，
    /// 会看到"预鉴权中间件"这一步已在 [Authorize] 判定之前执行——
    /// 证明"在鉴权之前做事"的正确姿势是中间件，而不是过滤器。
    /// </summary>
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure() => Ok(new { message = "只有通过鉴权才能看到我" });
}