namespace RequestPipelineDemo;

/// <summary>
/// 用来记录"一个请求从进来到出去经过了哪些步骤"的盒子。
/// 每个中间件/过滤器/控制器都往里写一行，方便直观看到管道执行顺序。
/// 它是单例（Singleton），所有请求共用一份，纯教学用途。
/// </summary>
public class PipelineTrace
{
    public List<string> Steps { get; } = [];

    public void Add(string step)
        => Steps.Add($"{DateTime.Now:HH:mm:ss.fff}  {step}");
}
