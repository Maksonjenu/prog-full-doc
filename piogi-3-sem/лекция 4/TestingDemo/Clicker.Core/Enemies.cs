namespace Clicker.Core;

public interface IEnemy
{
    string Name { get; }
    BigNumber CurrentLife { get; }
    BigNumber Gold { get; }
    bool IsDead { get; }

    /// <summary>Наносит урон. Возвращает true, если этим ударом противник побеждён.</summary>
    bool TakeDamage(BigNumber damage);
}

public abstract class CEnemy : IEnemy
{
    public string Name { get; }
    public BigNumber CurrentLife { get; private set; }
    public BigNumber Gold { get; }
    public bool IsDead => CurrentLife <= BigNumber.Zero;

    protected CEnemy(string name, BigNumber life, BigNumber gold)
    {
        Name = name;
        CurrentLife = life;
        Gold = gold;
    }

    public virtual bool TakeDamage(BigNumber damage)
    {
        if (damage < BigNumber.Zero)
            throw new ArgumentException("Урон не может быть отрицательным", nameof(damage));

        // ДЕМО ДЛЯ ЛЕКЦИИ: замените две строки ниже на
        //     CurrentLife.Subtract(damage);
        // и запустите тесты. BigNumber неизменяемый, результат вычитания выброшен —
        // здоровье не уменьшится, и EnemyTests сразу покраснеют.
        var rest = CurrentLife - damage;
        CurrentLife = rest < BigNumber.Zero ? BigNumber.Zero : rest;

        return IsDead;
    }
}

public class CNormalEnemy : CEnemy
{
    public CNormalEnemy(string name, BigNumber life, BigNumber gold) : base(name, life, gold) { }
}

/// <summary>Бронированный: броня вычитается из каждого удара, но урон не уходит в минус.</summary>
public class CArmoredEnemy : CEnemy
{
    public BigNumber Armor { get; }

    public CArmoredEnemy(string name, BigNumber life, BigNumber gold, BigNumber armor)
        : base(name, life, gold) => Armor = armor;

    public override bool TakeDamage(BigNumber damage)
    {
        var effective = damage > Armor ? damage - Armor : BigNumber.Zero;
        return base.TakeDamage(effective);
    }
}
