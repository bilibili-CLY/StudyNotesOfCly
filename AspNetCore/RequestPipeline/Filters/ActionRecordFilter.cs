using Microsoft.AspNetCore.Mvc.Filters;

namespace RequestPipelineDemo;

/// <summary>
/// 全局【Action 过滤器】：在控制器方法执行前/后各插一脚。
/// 演示"过滤器"这个环节在管道里的位置：路由选中了哪个 Action 之后、真正执行业务代码之前。
/// </summary>
public class ActionRecordFilter : IAsyncActionFilter
{
    private readonly PipelineTrace _trace;

    public ActionRecordFilter(PipelineTrace trace) => _trace = trace;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 执行控制器方法【之前】：此时模型绑定已完成、参数已填好
        _trace.Add($"[过滤器] 进入 Action 前：{context.ActionDescriptor.DisplayName}");

        // 模型绑定结果的验证在这里已经可用（ModelState）
        if (!context.ModelState.IsValid)
        {
            _trace.Add($"[过滤器] ⚠ ModelState 校验失败，字段：{string.Join(", ", context.ModelState.Keys)}");
        }

        var executed = await next(); // 这里才真正执行控制器里的方法

        // 执行控制器方法【之后】：拿到方法返回的结果对象
        _trace.Add($"[过滤器] Action 执行完，返回类型：{executed.Result?.GetType().Name}");
    }
}
