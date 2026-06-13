using UnityEngine;

[CreateAssetMenu(fileName = "NewBracers", menuName = "RPG/Bracers")]
public class BracersData : Item
{
    [Header("Бонуси наручів")]
    [Tooltip("Скільки максимального здоров'я додає (наприклад: 50)")]
    public int bonusMaxHealth = 0;

    [Tooltip("Скільки додаткового урону наносить (наприклад: 5)")]
    public int bonusDamage = 0;

    [Tooltip("На скільки відсотків збільшує швидкість бігу (наприклад: 0.2 для +20%)")]
    public float bonusMoveSpeed = 0f;

    [Tooltip("На скільки відсотків зменшує затримку між ударами (наприклад: 0.15 для +15% швидкості)")]
    public float bonusAttackSpeed = 0f;

    [Tooltip("Скільки ХП відновлює кожну секунду (наприклад: 2)")]
    public int healthRegenPerSecond = 0;

    [Header("Нові механіки (Кріт та Захист)")]
    [Tooltip("Шанс критичного удару (наприклад: 0.1 для 10%)")]
    public float bonusCritChance = 0f;

    [Tooltip("Множник критичного урону (наприклад: 2.0 для подвійного урону)")]
    public float bonusCritMultiplier = 2f;

    [Tooltip("Чисельне значення броні (наприклад: 40)")]
    public float bonusArmor = 0f;

    /// <summary>
    /// Перевизначений метод для розподілу бонусових показників наручів по трьох блоках UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання та Сила)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
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