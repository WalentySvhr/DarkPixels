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

    /// <summary>
    /// Перевизначений метод для відображення інформації про кільце в UI
    /// </summary>
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. БЛОК: Основні показники (Виживання та Сила)
        string main = "";
        main += $"Type: {type}\n";

        if (bonusMaxHealth > 0) main += $"Max HP: +{bonusMaxHealth}\n";
        if (bonusDamage > 0) main += $"Damage: +{bonusDamage}\n";

        // Додаємо лікування, якщо воно прописане в базі
        if (healValue > 0) main += $"Instant Heal: +{healValue}\n";

        desc.mainStats = main.TrimEnd();

        // 2. БЛОК: Короткий художній опис (з базового класу Item)
        desc.shortDesc = shortDescription;

        // 3. БЛОК: Спеціальні ефекти та швидкості
        string extra = "";

        if (bonusMoveSpeed > 0) extra += $"Move Speed: +{bonusMoveSpeed * 100}%\n";
        if (bonusAttackSpeed > 0) extra += $"Attack Speed: +{bonusAttackSpeed * 100}%\n";
        if (healthRegenPerSecond > 0) extra += $"Health Regen: {healthRegenPerSecond}/sec\n";

        desc.extraStats = extra.TrimEnd();

        // 4. БЛОК: Економіка
        if (price > 0)
        {
            desc.priceText = $"Price: {price} gold";
        }

        return desc;
    }
}