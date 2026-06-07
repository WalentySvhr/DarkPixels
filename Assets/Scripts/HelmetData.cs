using UnityEngine;

[CreateAssetMenu(fileName = "NewHelmet", menuName = "RPG/Helmet")]
public class HelmetData : Item
{
    [Header("Бонуси шолома")]
    [Tooltip("Скільки максимального здоров'я додає")]
    public int bonusMaxHealth = 0;

    [Tooltip("Додатковий захист (фізичний)")]
    public int bonusDefense = 0;

    [Tooltip("Відсоток поглинання урону (наприклад: 0.1 для 10%)")]
    public float bonusArmorPercent = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду")]
    public int healthRegenPerSecond = 0;

    [Header("Бойові показники")]
    [Tooltip("Шанс критичного удару (наприклад: 0.05 для 5%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник критичного урону")]
    public float bonusCritMultiplier = 2f;

    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Захист та Здоров'я)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusDefense > 0) main += $"Defense: +{bonusDefense}\n";
        if (bonusArmorPercent > 0) main += $"Armor: +{bonusArmorPercent * 100}%\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Художній опис
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти
        string extra = "";

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