using UnityEngine;

[CreateAssetMenu(fileName = "NewAmulet", menuName = "RPG/Amulet")]
public class AmuletData : Item
{
    [Header("Бонуси амулета")]
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

    /// <summary>
    /// Перевизначений метод для розподілу бонусів амулета по трьох блоках UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання та Сила)
        string main = "";
        // ДОДАЄМО ТИП ПРЕДМЕТА (як у зброї)
        // Використовуємо стандартне поле 'type', яке є в Item, для визначення категорії амулета
        main += $"Type: {type}\n";
        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";

        // Додаємо лікування, якщо воно прописане в базі
        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Спеціальні ефекти та швидкості
        string extra = "";

        // Форматуємо відсотки (наприклад, 0.2 -> 20%)
        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";
        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";

        desc.extraStats = extra.TrimEnd();

        // 3. БЛОК: Економіка
        if (price > 0)
        {
            desc.priceText = $"Price: {price} gold";
        }

        return desc;
    }
}