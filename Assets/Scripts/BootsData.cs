using UnityEngine;

[CreateAssetMenu(fileName = "NewBoots", menuName = "RPG/Boots")]
public class BootsData : Item
{
    [Header("Бонуси чобіт")]
    [Tooltip("На скільки відсотків збільшує швидкість бігу (наприклад: 0.15 для +15% швидкості)")]
    public float bonusMoveSpeed = 0f;

    [Tooltip("Скільки максимального здоров'я додає (наприклад: 30)")]
    public int bonusMaxHealth = 0;

    [Tooltip("Скільки максимальної мани додає (наприклад: 20)")]
    public int bonusMaxMana = 0;

    [Tooltip("Скільки додаткового урону наносить (наприклад: 2)")]
    public int bonusDamage = 0;

    [Tooltip("На скільки відсотків зменшує затримку між ударами (наприклад: 0.1 для +10% швидкості)")]
    public float bonusAttackSpeed = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду (наприклад: 1)")]
    public int healthRegenPerSecond = 0;

    [Tooltip("Скільки мани відновлює кожну секунду (наприклад: 1)")]
    public int manaRegenPerSecond = 0;

    [Header("Нові механіки (Кріт та Захист)")]
    [Tooltip("Чисельне значення броні (наприклад: 25)")]
    public float bonusArmor = 0f;

    [Tooltip("Шанс критичного удару (наприклад: 0.05 для 5%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник критичного урону (наприклад: 2.0 для подвійного урону)")]
    public float bonusCritMultiplier = 2f;

    /// <summary>
    /// Перевизначений метод для розподілу бонусових показників чобіт по трьох блоках UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання та Сила)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusMaxMana > 0) main += $"Max Mana: +{bonusMaxMana}\n";
        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";

        // Відображення чисельної броні в основних показниках
        if (bonusArmor > 0) main += $"Armor: +{bonusArmor}\n";
        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Короткий художній опис (з базового класу Item)
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти та швидкості
        string extra = "";

        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";

        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";
        if (manaRegenPerSecond > 0) extra += $"Mana Regen: {manaRegenPerSecond}/sec\n";

        // Відображення критів у спеціальних ефектах
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