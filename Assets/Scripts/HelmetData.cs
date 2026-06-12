using UnityEngine;

[CreateAssetMenu(fileName = "NewHelmet", menuName = "RPG/Helmet")]
public class HelmetData : Item
{
    [Header("Бонуси шолома")]
    [Tooltip("Скільки максимального здоров'я додає")]
    public int bonusMaxHealth = 0;

    [Tooltip("Чисельне значення броні (наприклад: 40)")]
    public float bonusArmor = 0f;

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

        // Відображення чисельної броні в основних показниках
        if (bonusArmor > 0) main += $"Armor: +{bonusArmor}\n";

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