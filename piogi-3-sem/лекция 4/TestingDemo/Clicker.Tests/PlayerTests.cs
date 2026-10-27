using Clicker.Core;

namespace Clicker.Tests;

[TestFixture]
public class PlayerTests
{
    private static CPlayer PlayerWithGold(string gold) =>
        new(new BigNumber(gold), damage: new BigNumber("1"), upgradeCost: new BigNumber("100"));

    [Test]
    public void TryUpgrade_NotEnoughGold_ReturnsFalseAndKeepsState()
    {
        var player = PlayerWithGold("99");

        bool upgraded = player.TryUpgrade();

        Assert.That(upgraded, Is.False);
        Assert.That(player.Gold.ToString(), Is.EqualTo("99"));
        Assert.That(player.Level, Is.EqualTo(1));
    }

    // Именно этот тест «убивает» мутанта  Gold < UpgradeCost  →  Gold <= UpgradeCost.
    // Закомментируйте его — и Stryker покажет выжившего мутанта, хотя все остальные тесты зелёные.
    [Test]
    public void TryUpgrade_ExactlyEnoughGold_UpgradesAndSpendsAll()
    {
        var player = PlayerWithGold("100");

        bool upgraded = player.TryUpgrade();

        Assert.That(upgraded, Is.True);
        Assert.That(player.Gold.ToString(), Is.EqualTo("0"));
    }

    [Test]
    public void TryUpgrade_EnoughGold_DoublesDamageAndCost()
    {
        var player = PlayerWithGold("150");

        player.TryUpgrade();

        // Assert.Multiple: проверяются ВСЕ условия, даже если первое упало, — видно сразу всю картину
        Assert.Multiple(() =>
        {
            Assert.That(player.Level, Is.EqualTo(2));
            Assert.That(player.Damage.ToString(), Is.EqualTo("2"));
            Assert.That(player.UpgradeCost.ToString(), Is.EqualTo("200"));
            Assert.That(player.Gold.ToString(), Is.EqualTo("50"));
        });
    }

    [Test]
    public void AddGold_Negative_ThrowsArgumentException()
    {
        var player = PlayerWithGold("0");

        var ex = Assert.Throws<ArgumentException>(() => player.AddGold(new BigNumber("-1")));

        Assert.That(ex!.ParamName, Is.EqualTo("amount"));
    }
}
