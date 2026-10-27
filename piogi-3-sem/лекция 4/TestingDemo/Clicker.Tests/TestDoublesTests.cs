using Clicker.Core;

namespace Clicker.Tests;

// ---------- Тестовые дублёры, написанные руками ----------

/// <summary>Stub (заглушка): всегда возвращает заранее заданное «случайное» число.</summary>
public class FixedRandom : IRandomSource
{
    private readonly double _value;
    public FixedRandom(double value) => _value = value;
    public double NextDouble() => _value;
}

/// <summary>Spy (шпион): ничего не пишет на диск, только запоминает, как его вызывали.</summary>
public class SpySaver : ISaveList<CPlayer>
{
    public int SaveCalls { get; private set; }
    public string? LastPath { get; private set; }

    public void Save(CPlayer data, string path)
    {
        SaveCalls++;
        LastPath = path;
    }

    public CPlayer Load(string path) => throw new NotSupportedException("В этих тестах загрузка не нужна");
}

[TestFixture]
public class EnemySpawnerTests
{
    private static readonly EnemyTemplate[] Templates =
    {
        new("Слизень", 70),   // после нормализации: [0 .. 0.7]
        new("Рыцарь", 25),    //                     (0.7 .. 0.95]
        new("Дракон", 5),     //                     (0.95 .. 1)
    };

    // С настоящим Random тест был бы недетерминированным: то зелёный, то красный.
    // Подставляем заглушку — «случайность» становится управляемой.
    [TestCase(0.0, "Слизень")]
    [TestCase(0.7, "Слизень")]     // граница: ровно 0.7 — ещё слизень (sum >= roll)
    [TestCase(0.71, "Рыцарь")]
    [TestCase(0.96, "Дракон")]
    [TestCase(0.9999, "Дракон")]
    public void Next_UsesRollToPickTemplate(double roll, string expectedName)
    {
        var spawner = new EnemySpawner(Templates, new FixedRandom(roll));

        var enemy = spawner.Next();

        Assert.That(enemy.Name, Is.EqualTo(expectedName));
    }

    [Test]
    public void Constructor_NoTemplates_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EnemySpawner(Array.Empty<EnemyTemplate>(), new FixedRandom(0.5)));
    }
}

[TestFixture]
public class GameSessionTests
{
    [Test]
    public void Upgrade_Success_SavesProgressOnce()
    {
        var player = new CPlayer(new BigNumber("150"), new BigNumber("1"), new BigNumber("100"));
        var saver = new SpySaver();
        var session = new GameSession(player, saver, "save.json");

        session.Upgrade();

        Assert.That(saver.SaveCalls, Is.EqualTo(1));
        Assert.That(saver.LastPath, Is.EqualTo("save.json"));
    }

    [Test]
    public void Upgrade_NotEnoughGold_DoesNotSave()
    {
        var player = new CPlayer(new BigNumber("0"), new BigNumber("1"), new BigNumber("100"));
        var saver = new SpySaver();
        var session = new GameSession(player, saver, "save.json");

        session.Upgrade();

        Assert.That(saver.SaveCalls, Is.Zero);
    }
}
