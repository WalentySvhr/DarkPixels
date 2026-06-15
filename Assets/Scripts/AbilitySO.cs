using UnityEngine;

public enum AbilityType { Instant, Toggleable } // Разовий або Активний

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Game Ability")]
public class AbilitySO : ScriptableObject
{
    [Header("Загальні Налаштування")]
    public string abilityName;

    [TextArea(3, 10)]
    [Tooltip("Текст опису. Можна юзати теги {name}, {level}, {damage}, {radius}, {price}")]
    public string descriptionTemplate;

    public Sprite abilityIcon; // Іконка для префабу та UI кнопки
    public AbilityType type;

    [Header("Економіка (Вартість манни)")]
    [Tooltip("Для Instant — ціна касту. Для Toggleable — ціна в секунду.")]
    public float manaCost;

    [Tooltip("На скільки збільшується вартість мани за кожен новий рівень (після 1-го)")]
    public float manaCostIncreasePerLevel = 2f; // <-- НОВА ЗМІННА З ОПТИМАЛЬНИМ ДЕФОЛТОМ

    [Header("Бойові Характеристики (Базові)")]
    [Tooltip("Стартова шкода 1-го рівня (за один раз або за один Тік аури)")]
    public int baseDamage;

    [Tooltip("Радіус дії зони ураження навколо гравця")]
    public float radius = 3f;

    [Tooltip("Частота нанесення урону (Тік) в секундах для Toggleable аури.")]
    public float tickRate = 1f;

    [Tooltip("Сила відкидання ворога від центру вміння. Якщо поставити 0 — відкидання не буде.")]
    public float knockbackForce = 0f;

    [Header("Прогресія прокачування за Коіни")]
    [Tooltip("Початкова вартість прокачування (перша покупка, щоб отримати 1 рівень)")]
    public int baseUpgradeCost = 100;

    [Tooltip("На скільки дорожчає кожне наступне покращення")]
    public int costIncreasePerLevel = 50;

    [Tooltip("Скільки чистого урону додається до бази за кожен новий рівень")]
    public int damageIncreasePerLevel = 5;

    [Header("Візуальні Ефекти (FX)")]
    [Tooltip("Префаб графіки скіла (наприклад, вогняне коло, частинки тощо)")]
    public GameObject visualEffectPrefab;

    // Початковий стан тепер 0 (вміння заблоковане до моменту покупки)
    [HideInInspector][SerializeField] public int currentLevel = 0;

    // --- ОНОВЛЕНІ ФОРМУЛИ ДИНАМІЧНОГО РОЗРАХУНКУ ---

    /// <summary>
    /// Розраховує актуальну вартість мани залежно від рівня. Якщо рівень 0 — повертає 0.
    /// </summary>
    public float GetCurrentManaCost() // <-- НОВИЙ МЕТОД
    {
        if (currentLevel <= 0) return 0f;
        return manaCost + (currentLevel - 1) * manaCostIncreasePerLevel;
    }

    /// <summary>
    /// Розраховує актуальну шкоду. Якщо рівень 0 — повертає 0.
    /// </summary>
    public int GetCurrentDamage()
    {
        if (currentLevel <= 0) return 0;
        return baseDamage + (currentLevel - 1) * damageIncreasePerLevel;
    }

    /// <summary>
    /// Розраховує вартість наступного апгрейду. 
    /// Якщо рівень 0 (перша покупка) — повертає базову ціну.
    /// </summary>
    public int GetUpgradeCost()
    {
        if (currentLevel <= 0) return baseUpgradeCost;
        return baseUpgradeCost + (currentLevel) * costIncreasePerLevel;
    }

    // --- МЕТОДИ ДЛЯ ІНТЕГРАЦІЇ З SAVEMANAGER ---

    /// <summary>
    /// Повертає унікальний текстовий ключ для збереження цього конкретного вміння.
    /// </summary>
    public string GetSaveKey()
    {
        string cleanName = string.IsNullOrEmpty(abilityName) ? name : abilityName;
        return "Ability_" + cleanName + "_Level";
    }

    /// <summary>
    /// Метод для завантаження рівня. Тепер дозволяє рівень 0, якщо гравець ще не купив скіл.
    /// </summary>
    public void SetLoadedLevel(int loadedLevel)
    {
        // Захист: рівень не може бути від'ємним
        currentLevel = Mathf.Max(0, loadedLevel);
    }
}