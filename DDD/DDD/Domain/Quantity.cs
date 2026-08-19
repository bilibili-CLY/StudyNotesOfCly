namespace DDDDemo.Domain;

/// <summary>值对象：数量。不可变、无标识，靠值本身相等，构造时校验。</summary>
public sealed record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0) throw new ArgumentException("数量必须大于 0");
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
