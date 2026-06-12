using UnityEngine;

[CreateAssetMenu(fileName = "NewChestplate", menuName = "RPG/Chestplate")]
public class ChestplateData : Item
{
    [Header("Бонуси нагрудника")]
    [Tooltip("Скільки максимального здоров'я додає")]
    public int bonusMaxHealth = 0;

    [Tooltip("Скільки додаткового урону наносить")]
    public int bonusDamage = 0;

    [Tooltip("На скільки відсотків збільшує швидкість бігу (наприклад: 0.2 для +20%)")]
    public float bonusMoveSpeed = 0f;

    [Tooltip("На скільки відсотків зменшує затримку між ударами (наприклад: 0.15 для +15% швидкості)")]
    public float bonusAttackSpeed = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду")]
    public int healthRegenPerSecond = 0;

    [Header("Механіки (Кріт та Захист)")]
    [Tooltip("Шанс критичного удару (наприклад: 0.1 для 10%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник критичного урону (наприклад: 2.0 для подвійного урону)")]
    public float bonusCritMultiplier = 2f;

    [Tooltip("Чисельне значення броні")]
    public float bonusArmor = 0f;

    /// <summary>
    /// Перевизначений метод для відображення характеристик нагрудника
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";
        if (bonusArmor > 0) main += $"Armor: +{bonusArmor}\n";
        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Короткий художній опис
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти та швидкості
        string extra = "";

        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";
        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";
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