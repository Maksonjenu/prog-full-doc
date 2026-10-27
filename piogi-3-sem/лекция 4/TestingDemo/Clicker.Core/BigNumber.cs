using System.Numerics;

namespace Clicker.Core;

/// <summary>
/// Упрощённый BigNumber для демо лекции 4. В Лабе 2 вы пишете свой — на массиве блоков по основанию 1000.
/// Здесь внутри System.Numerics.BigInteger: лекция про тесты, а не про арифметику,
/// и тестам всё равно, как класс устроен внутри, — они проверяют только поведение снаружи.
/// Как и в Лабе 2, Equals НЕ переопределён: сравнивать объекты в тестах нужно через ToString().
/// </summary>
public sealed class BigNumber : IComparable<BigNumber>
{
    private readonly BigInteger _value;

    public BigNumber(string digits)
    {
        if (string.IsNullOrWhiteSpace(digits) || !BigInteger.TryParse(digits, out _value))
            throw new FormatException($"'{digits}' — не число");
    }

    private BigNumber(BigInteger value) => _value = value;

    public static BigNumber Zero => new(BigInteger.Zero);

    // Немутирующие операции: возвращают НОВЫЙ объект, исходный не меняется (immutability из Лабы 2)
    public BigNumber Add(BigNumber other) => new(_value + other._value);
    public BigNumber Subtract(BigNumber other) => new(_value - other._value);
    public BigNumber Multiply(BigNumber other) => new(_value * other._value);

    public BigNumber Divide(BigNumber other)
    {
        if (other._value.IsZero)
            throw new DivideByZeroException("Деление BigNumber на ноль");
        return new(_value / other._value);
    }

    public static BigNumber operator +(BigNumber a, BigNumber b) => a.Add(b);
    public static BigNumber operator -(BigNumber a, BigNumber b) => a.Subtract(b);
    public static BigNumber operator *(BigNumber a, BigNumber b) => a.Multiply(b);
    public static BigNumber operator /(BigNumber a, BigNumber b) => a.Divide(b);

    public int CompareTo(BigNumber? other) => other is null ? 1 : _value.CompareTo(other._value);
    public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
    public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
    public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;

    public override string ToString() => _value.ToString();
}
