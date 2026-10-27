namespace Clicker.Core;

public class CPlayer
{
    private static readonly BigNumber Two = new("2");

    public BigNumber Gold { get; private set; }
    public BigNumber Damage { get; private set; }
    public BigNumber UpgradeCost { get; private set; }
    public int Level { get; private set; } = 1;

    public CPlayer(BigNumber gold, BigNumber damage, BigNumber upgradeCost)
    {
        Gold = gold;
        Damage = damage;
        UpgradeCost = upgradeCost;
    }

    public void AddGold(BigNumber amount)
    {
        if (amount < BigNumber.Zero)
            throw new ArgumentException("Нельзя добавить отрицательное золото", nameof(amount));
        Gold = Gold + amount;
    }

    /// <summary>Пытается купить улучшение урона. false — если золота не хватает (это не ошибка, а обычная ситуация).</summary>
    public bool TryUpgrade()
    {
        if (Gold < UpgradeCost)   // мутант для Stryker: "<" → "<=". Его убивает только тест на «ровно хватает»
            return false;

        Gold = Gold - UpgradeCost;
        Level++;
        Damage = Damage * Two;
        UpgradeCost = UpgradeCost * Two;
        return true;
    }
}
