namespace CSharpPractice.Delegate;

public static class DelegatePrac
{
    public static void Execute()
    {
        _send("你好");
    }

    private delegate bool Send(string msg);

    private static Send _send = SendMsg;
    private static bool SendMsg(string msg)
    {
        Console.WriteLine($"假装发送了msg，内容{msg}");
        // 成功返回true
        return true;
    }
}