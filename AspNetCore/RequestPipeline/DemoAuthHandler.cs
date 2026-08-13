using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RequestPipelineDemo;

/// <summary>
/// 演示用的"假认证"处理器：从不认证任何请求。
/// 目的：让 [Authorize] 走完整个授权流程并返回 401（而不是因为没有认证方案而抛异常），
/// 从而能观察"PreAuthFilter 在 [Authorize] 之前执行"。
/// 真实项目里这里会是 JWT / Cookie 等真正的认证方案。
/// </summary>
public class DemoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DemoAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    // 返回 NoResult = "没有用户登录"，于是带 [Authorize] 的接口会被拦成 401
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}