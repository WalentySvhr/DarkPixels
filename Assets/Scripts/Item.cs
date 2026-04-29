using UnityEngine;

// Категорії предметів
public enum ItemType { Food, Potion, Resource, Junk, Weapon, Amulet }

// Структура для розділеного опису предмета
public struct ItemDescription
{
    public string mainStats;   // Блок 1: Тип та основна дія (напр. Лікування)
    public string extraStats;  // Блок 2: Опис або додаткові ефекти
    public string priceText;   // Блок 3: Ціна
}

public class Item : ScriptableObject
{
    [Header("Загальна інформація")]
    public string itemName;
    public ItemType type;
    public Sprite icon;
    public int price;

    public ItemType category; // Додай це поле

    [Header("Налаштування стаку")]
    public bool isStackable;
    public int maxStackSize = 10;

    [Header("Ефекти (якщо є)")]
    public int healValue = 0;

    /// <summary>
    /// Формує детальний опис для будь-якого базового предмета (їжа, зілля, ресурси)
    /// </summary>
    public virtual ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. Визначаємо назву типу українською
        string typeDisplay = GetUkrainianTypeName();

        // Формуємо основний блок (Тип + характеристика)
        string main = $"Type: {typeDisplay}\n";

        if (healValue > 0)
        {
            // Для їжі та зілля додаємо показник лікування
            main += $"Heal: +{healValue} HP";
        }
        else if (type == ItemType.Resource)
        {
            main += "Material for crafting";
        }
        else if (type == ItemType.Junk)
        {
            main += "Has no practical value";
        }

        desc.mainStats = main;

        // 2. Блок додаткової інформації (наприклад, про стак)
        if (isStackable)
        {
            desc.extraStats = $"Stackable up to: {maxStackSize} items";
        }

        // 3. Блок ціни
        if (price > 0)
        {
            desc.priceText = $"Price: {price} gold";
        }

        return desc;
    }

    // Допоміжний метод для гарного відображення типу
    private string GetUkrainianTypeName()
    {
        switch (type)
        {
            case ItemType.Food: return "Food";
            case ItemType.Potion: return "Potion";
            case ItemType.Resource: return "Resource";
            case ItemType.Junk: return "Junk";
            case ItemType.Weapon: return "Weapon";
            case ItemType.Amulet: return "Amulet";
            default: return "Item";
        }
    }

    public virtual string GetInfoText() => GetDetailedInfo().mainStats;
}