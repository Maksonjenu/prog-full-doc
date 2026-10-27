using Clicker.Core;

namespace Clicker.Tests;

[TestFixture]
public class BigNumberTests
{
    // Параметризованный тест: один метод — много сценариев, каждый [TestCase] отдельной строкой в Обозревателе тестов
    [TestCase("2", "2", "4")]                       // обычный случай
    [TestCase("999", "1", "1000")]                  // граница блока: перенос в новый разряд
    [TestCase("3712117", "15512900", "19225017")]   // перенос через два разряда (разбор из Лабы 2)
    [TestCase("0", "0", "0")]                       // ноль
    public void Add_TwoNumbers_ReturnsSum(string a, string b, string expected)
    {
        // Arrange
        var left = new BigNumber(a);
        var right = new BigNumber(b);

        // Act
        var result = left + right;

        // Assert — сравниваем строковое представление: Equals у BigNumber не переопределён
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase("3712117", "1911900", "1800217")]     // заём через два разряда подряд
    [TestCase("1000", "1", "999")]                  // заём через весь блок
    [TestCase("5", "8", "-3")]                      // отрицательный результат
    public void Subtract_TwoNumbers_ReturnsDifference(string a, string b, string expected)
    {
        var result = new BigNumber(a) - new BigNumber(b);

        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase("10014217", "3", "3338072")]          // деление с остатком: остаток отбрасывается
    public void Divide_TwoNumbers_ReturnsIntegerQuotient(string a, string b, string expected)
    {
        var result = new BigNumber(a) / new BigNumber(b);

        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var number = new BigNumber("100");
        var zero = new BigNumber("0");

        // Вызов обёрнут в лямбду: иначе исключение вылетит раньше, чем Assert успеет его поймать
        var ex = Assert.Throws<DivideByZeroException>(() => { _ = number / zero; });

        Assert.That(ex!.Message, Does.Contain("ноль"));
    }

    [Test]
    public void Add_DoesNotChangeOperands()   // проверяем неизменяемость (immutability)
    {
        var a = new BigNumber("10");
        var b = new BigNumber("5");

        _ = a + b;

        Assert.That(a.ToString(), Is.EqualTo("10"));
    }

    [TestCase("")]
    [TestCase("abc")]
    public void Constructor_NotANumber_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => new BigNumber(input));
    }
}
