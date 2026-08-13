using Microsoft.AspNetCore.Authentication;
using RequestPipelineDemo;

// ============================================================
//  这一段就是"程序启动 + 组装请求管道"的地方。
//  所有的中间件（Middleware）都按顺序排队，构成了一个管道，
//  每个请求进网线后，就像流水线上的一件货品，按顺序穿过它们。
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ---- 第 1 步：登记依赖注入（DI）----
// 告诉框架："以后谁要 PipelineTrace / StockService，就给它创建并送进去"
builder.Services.AddSingleton<PipelineTrace>();
builder.Services.AddScoped<StockService>();

// ---- 第 2 步：开启 MVC（控制器）----
// AddControllers 一句话就把：路由、模型绑定、结果格式化、过滤器、DI 全装上。
// 这里顺便挂上我们的全局过滤器（每个控制器方法执行前/后都会跑）。

// 先注册一个"演示用认证方案"，让 [Authorize] 能正常走流程并返回 401
// （真实项目里是 AddJwtBearer / AddCookie 等真正的方案）。
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Demo";           // 匿名判断用哪个方案
    options.DefaultChallengeScheme = "Demo";  // 未通过鉴权时用哪个方案发起质询
})
.AddScheme<AuthenticationSchemeOptions, DemoAuthHandler>("Demo", null);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<PreAuthFilter>();       // 全局授权过滤器：先于 action 上的 [Authorize]
    options.Filters.Add<ActionRecordFilter>();
});

var app = builder.Build();

// ============================================================
//  下面按书写顺序，把中间件一个接一个排进"请求管道"。
//  顺序就是执行顺序，别排错，否则行为会不一样。
// ============================================================

// ---- 中间件 ①：全局异常兜底 ----（最外层，包住后面所有环节）
// 里面用 try/catch 把"后面所有环节"包起来：
// 任何环节抛异常，都会被这里抓住，转成一个 500 响应，不让请求裸奔崩溃。
app.Use(async (ctx, next) =>
{
    var trace = ctx.RequestServices.GetRequiredService<PipelineTrace>();
    try
    {
        trace.Add("【中间件①-异常兜底】请求进入（开始 try 保护后面所有环节）");
        await next();
        trace.Add($"【中间件①-异常兜底】正常返回，状态码 {ctx.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        trace.Add($"【中间件①-异常兜底】💥 捕获到异常：{ex.GetType().Name} -> {ex.Message}");
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "服务器出错了（全局异常中间件兜底响应）" });
    }
});

// ---- 中间件 ②：请求日志/计时 ----
app.Use(async (ctx, next) =>
{
    var pipeline = ctx.RequestServices.GetRequiredService<PipelineTrace>();
    var sw = System.Diagnostics.Stopwatch.StartNew();
    pipeline.Add($"【中间件②-日志】收到请求：{ctx.Request.Method} {ctx.Request.Path}");
    Console.WriteLine($"==> 请求进入：{ctx.Request.Method} {ctx.Request.Path}");

    await next(); // 放行给"后面"的环节

    sw.Stop();
    pipeline.Add($"【中间件②-日志】响应离开，耗时 {sw.ElapsedMilliseconds} ms，状态码 {ctx.Response.StatusCode}");
});

// ---- 中间件 ③：启动路由 ----
// 路由负责看 URL，决定"这个请求该交给哪个控制器的方法处理"。
// 这一步之前中间件只管"过路"，这一步才会把请求"指派"到某个 Action。
app.UseRouting();

// ---- 中间件 ④：预鉴权中间件 ----
// 【答案】想"在鉴权之前做操作"，就写一个中间件，放在 UseAuthentication /
// UseAuthorization 之前。此刻请求还没开始鉴权，天然在 [Authorize] 之前。
// 此时路由已匹配完成，还能通过 ctx.GetEndpoint() 拿到"目标是谁"。
app.Use(async (ctx, next) =>
{
    var trace = ctx.RequestServices.GetRequiredService<PipelineTrace>();
    var endpoint = ctx.GetEndpoint();
    trace.Add($"[预鉴权中间件] 鉴权开始前！目标终结点：{endpoint?.DisplayName ?? "(无)"}");
    await next();
});

// ---- 中间件 ⑤：认证 / 授权（AuthorizeFilter 的执行前提）----
// 想用 [Authorize] 就必须在路由之后、终点之前启用它们。
// app.UseAuthentication() 识别"你是谁"；app.UseAuthorization() 决定"你能否进来"。
// 注意：对挂 [Authorize] 的接口，鉴权判定在这里就短路返回 401，
// 根本走不到后面的 MVC 过滤器管道 —— 所以"在 [Authorize] 之前"要用中间件④。
app.UseAuthentication();
app.UseAuthorization();

// ---- 第 3 步：绑定终结点（Endpoint）----
// MapControllers 把控制器里的每个 Action 注册成一个"终点"（Endpoint），
// 这样路由就能匹配上了。MVC 的请求旅程从这儿开始"对症下药"。
app.MapControllers();

// ---- 第 4 步：真正开始监听请求 ----
app.Run();

// 现在运行 dotnet run，然后另开一个终端发请求，就能看到管道顺序了。