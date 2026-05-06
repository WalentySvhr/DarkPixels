using UnityEngine;

[CreateAssetMenu(fileName = "NewRing", menuName = "RPG/Ring")]
public class RingData : Item
{
    [Header("Бонуси кільця")]
    [Tooltip("Скільки максимального здоров'я додає")]
    public int bonusMaxHealth = 0;

    [Tooltip("Скільки додаткового урону наносить")]
    public int bonusDamage = 0;

    [Tooltip("На скільки відсотків збільшує швидкість бігу (наприклад: 0.1 для +10%)")]
    public float bonusMoveSpeed = 0f;

    [Tooltip("На скільки відсотків збільшує швидкість атаки (наприклад: 0.1 для +10%)")]
    public float bonusAttackSpeed = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду")]
    public int healthRegenPerSecond = 0;

    [Header("Нові механіки")]
    [Range(0, 1f)]
    [Tooltip("Шанс критичного удару (0.1 = 10%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник урону при кріті (наприклад: 2 = подвійний урон)")]
    public float bonusCritMultiplier = 2f;

    [Range(0, 1f)]
    [Tooltip("Відсоток поглинання урону (0.2 = -20% урону)")]
    public float bonusArmorPercent = 0f;

    /// <summary>
    /// Перевизначений метод для відображення інформації про кільце в UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання, Сила та Захист)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";

        // Відображаємо броню в основних статах
        if (bonusArmorPercent > 0) main += $"Armor: +{bonusArmorPercent * 100}%\n";

        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Короткий художній опис
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти, швидкості та крити
        string extra = "";

        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";
        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";

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