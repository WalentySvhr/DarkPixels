using UnityEngine;

[CreateAssetMenu(fileName = "NewRing", menuName = "RPG/Ring")]
public class RingData : Item
{
    [Header("Бонуси кільця")]
    [Tooltip("Скільки максимального здоров'я додає")]
    public int bonusMaxHealth = 0;

    // --- ДОДАНО: Бонус мани ---
    [Tooltip("Скільки максимальної мани додає (наприклад: 15)")]
    public int bonusMaxMana = 0;

    [Tooltip("Скільки додаткового урону наносить")]
    public int bonusDamage = 0;

    [Tooltip("На скільки відсотків збільшує швидкість бігу (наприклад: 0.1 для +10%)")]
    public float bonusMoveSpeed = 0f;

    [Tooltip("На скільки відсотків збільшує швидкість атаки (наприклад: 0.1 для +10%)")]
    public float bonusAttackSpeed = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду")]
    public int healthRegenPerSecond = 0;

    // --- ДОДАНО: Регенерація мани ---
    [Tooltip("Скільки мани відновлює кожну секунду (наприклад: 1)")]
    public int manaRegenPerSecond = 0;

    [Header("Нові механіки")]
    [Range(0, 1f)]
    [Tooltip("Шанс критичного удару (0.1 = 10%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник урону при кріті (наприклад: 2 = подвійний урон)")]
    public float bonusCritMultiplier = 2f;

    [Tooltip("Чисельне значення броні (наприклад: 15)")]
    public float bonusArmor = 0f;

    /// <summary>
    /// Перевизначений метод для відображення інформації про кільце в UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання, Мана, Сила та Захист)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";

        // --- ДОДАНО: Вивід мани в основні стати кільця ---
        if (bonusMaxMana > 0) main += $"Max Mana: +{bonusMaxMana}\n";

        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";

        // Відображаємо чисельну броню в основних статах (без знаку %)
        if (bonusArmor > 0) main += $"Armor: +{bonusArmor}\n";

        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Короткий художній опис
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти, швидкості та крити
        string extra = "";

        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";
        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";

        // --- ДОДАНО: Вивід регену мани ---
        if (manaRegenPerSecond > 0) extra += $"Mana Regen: {manaRegenPerSecond}/sec\n";

        // Відображаємо крити в додаткових ефектах
        if (bonusCritChance > 0) extra += $"Crit Chance: +{bonusCritChance * 100}%\n";
        if (bonusCritMultiplier > 2f) extra += $"Crit Multiplier: x{bonusCritMultiplier}\n";

        desc.extraStats = extra.TrimEnd();

        // 4. БЛОК: Економіка
        if (price > 0)
        {
            desc.priceText = $"Price: {price} gold";
        }

        return desc;
    }
}