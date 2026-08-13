using Microsoft.AspNetCore.Mvc.Filters;

namespace RequestPipelineDemo;

/// <summary>
/// 自定义【授权过滤器】——MVC 过滤器管道里"最先执行"的一类过滤器。
/// 注意（重要）：它排在 MVC 过滤器管道最前面，但排在【中间件层】的
/// [Authorize] 判定【之后】——因为 UseAuthorization 中间件会在到达
/// MVC 过滤器管道之前就对 [Authorize] 短路返回 401。
///
/// 因此本过滤器适合做：① 对"所有能到达终点的请求"先做一层额外检查
/// （白名单、注入租户上下文、生成请求 ID）；② 先于方法/控制器级自定义
/// 授权过滤器执行（同类型过滤器：全局 > 控制器 > Action，可用 Order 调整）。
/// 如果想在 [Authorize] 之前做操作 → 请用中间件，放在 UseAuthorization 之前。
/// </summary>
public class PreAuthFilter : IAuthorizationFilter
{
    private readonly PipelineTrace _trace;

    public PreAuthFilter(PipelineTrace trace) => _trace = trace;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 走到这里说明请求已通过中间件层的鉴权、真正到达了 MVC 管道。
        // 在模型绑定和业务代码之前先干点活（真实场景：白名单/租户/请求ID）。
        context.HttpContext.Items["RequestId"] = Guid.NewGuid();
        _trace.Add("[授权过滤器] PreAuthFilter：MVC 管道内的第一道授权关卡（先于模型绑定与业务代码）");
    }
}