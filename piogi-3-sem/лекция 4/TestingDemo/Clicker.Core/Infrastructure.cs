namespace Clicker.Core;

// ---------- Случайность за интерфейсом: в тестах подменяется заглушкой ----------

public interface IRandomSource
{
    /// <summary>Число от 0 (включительно) до 1 (не включительно), как Random.NextDouble().</summary>
    double NextDouble();
}

public class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new();
    public double NextDouble() => _random.NextDouble();
}

public record EnemyTemplate(string Name, double SpawnChance);

/// <summary>Выбор следующего противника по шансам появления (алгоритм из материала Лабы 2).</summary>
public class EnemySpawner
{
    private readonly IReadOnlyList<EnemyTemplate> _templates;
    private readonly IRandomSource _random;
    private readonly double _totalChance;

    public EnemySpawner(IReadOnlyList<EnemyTemplate> templates, IRandomSource random)
    {
        if (templates.Count == 0)
            throw new ArgumentException("Нужен хотя бы один шаблон противника", nameof(templates));
        _templates = templates;
        _random = random;
        _totalChance = templates.Sum(t => t.SpawnChance);   // нормализация: сумма шансов → 1
    }

    public EnemyTemplate Next()
    {
        double roll = _random.NextDouble();
        double sum = 0;
        foreach (var template in _templates)
        {
            sum += template.SpawnChance / _totalChance;
            if (sum >= roll) return template;
        }
        return _templates[^1];   // страховка от погрешности double
    }
}

// ---------- Сохранение за интерфейсом (ISaveList<T> из Лабы 4) ----------

public interface ISaveList<T>
{
    T Load(string path);
    void Save(T data, string path);
}

/// <summary>Игровая сессия: после удачного апгрейда прогресс сохраняется автоматически.</summary>
public class GameSession
{
    private readonly CPlayer _player;
    private readonly ISaveList<CPlayer> _saver;
    private readonly string _savePath;

    public GameSession(CPlayer player, ISaveList<CPlayer> saver, string savePath)
    {
        _player = player;
        _saver = saver;
        _savePath = savePath;
    }

    public bool Upgrade()
    {
        bool upgraded = _player.TryUpgrade();
        if (upgraded)
            _saver.Save(_player, _savePath);
        return upgraded;
    }
}
