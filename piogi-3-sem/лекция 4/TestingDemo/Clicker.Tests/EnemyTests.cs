using Clicker.Core;

namespace Clicker.Tests;

[TestFixture]
public class EnemyTests
{
    private CNormalEnemy _slime = null!;

    // [SetUp] выполняется перед КАЖДЫМ тестом: у каждого теста свой свежий противник, тесты не влияют друг на друга
    [SetUp]
    public void CreateEnemy()
    {
        _slime = new CNormalEnemy("Слизень", new BigNumber("100"), new BigNumber("10"));
    }

    [Test]
    public void TakeDamage_LessThanLife_ReducesLife()
    {
        // Arrange — в SetUp: слизень со 100 HP

        // Act
        bool defeated = _slime.TakeDamage(new BigNumber("30"));

        // Assert
        Assert.That(_slime.CurrentLife.ToString(), Is.EqualTo("70"));
        Assert.That(defeated, Is.False);
    }

    [Test]
    public void TakeDamage_EqualToLife_EnemyIsDefeated()   // граница: урон ровно равен здоровью
    {
        bool defeated = _slime.TakeDamage(new BigNumber("100"));

        Assert.That(defeated, Is.True);
        Assert.That(_slime.CurrentLife.ToString(), Is.EqualTo("0"));
    }

    [Test]
    public void TakeDamage_MoreThanLife_LifeDoesNotGoNegative()
    {
        _slime.TakeDamage(new BigNumber("250"));

        Assert.That(_slime.CurrentLife.ToString(), Is.EqualTo("0"));
    }

    [Test]
    public void TakeDamage_Negative_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _slime.TakeDamage(new BigNumber("-5")));
    }
}

[TestFixture]
public class ArmoredEnemyTests
{
    // Классы эквивалентности для брони 10: урон меньше брони, равен броне, больше брони.
    // Граничные значения: 9, 10, 11 — ошибки чаще всего живут именно на границе ( > вместо >= ).
    [TestCase("0", "100")]
    [TestCase("9", "100")]
    [TestCase("10", "100")]
    [TestCase("11", "99")]
    [TestCase("50", "60")]
    public void TakeDamage_ArmorAbsorbsPartOfDamage(string damage, string expectedLife)
    {
        var knight = new CArmoredEnemy("Рыцарь", new BigNumber("100"), new BigNumber("50"), armor: new BigNumber("10"));

        knight.TakeDamage(new BigNumber(damage));

        Assert.That(knight.CurrentLife.ToString(), Is.EqualTo(expectedLife));
    }
}
