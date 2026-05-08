using UnityEngine;

// Категорії предметів
public enum ItemType { Food, Potion, Resource, Junk, Weapon, Amulet, Ring }

// Структура для розділеного опису предмета
public struct ItemDescription
{
    public string mainStats;   // Блок 1: Тип та основна дія
    public string shortDesc;   // Короткий художній опис
    public string extraStats;  // Блок 2: Додаткові ефекти або стак
    public string priceText;   // Блок 3: Ціна
}

[CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    [Header("Загальна інформація")]
    public string itemName;
    public ItemType type;
    public Sprite icon;
    public int price;

    [TextArea(2, 5)]
    public string shortDescription;

    [Header("Налаштування стаку")]
    public bool isStackable;
    public int maxStackSize = 10;

    [Header("Ефекти (якщо є)")]
    public int healValue = 0;

    // --- ДОДАНО: Налаштування квестів ---
    [Header("Квестові налаштування")]
    public bool isQuestItem = false;

    public virtual ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. Основні характеристики
        string typeDisplay = GetUkrainianTypeName();
        string main = $"Type: {typeDisplay}\n";

        if (healValue > 0)
            main += $"<color=green>Heal: +{healValue} HP</color>";
        else if (type == ItemType.Resource)
            main += "Material for crafting";
        else if (type == ItemType.Junk)
            main += "Has no practical value";
        else if (type == ItemType.Ring)
            main += "Magical accessory";

        // Додаємо позначку квестового предмета в основний опис
        if (isQuestItem)
        {
            main += "\n<color=magenta>Quest Item</color>";
        }

        desc.mainStats = main;
        desc.shortDesc = shortDescription;

        if (isStackable)
        {
            desc.extraStats = $"Stackable up to: {maxStackSize} items";
        }

        // --- ОНОВЛЕНО: Відображення ціни або заборони продажу ---
        if (isQuestItem)
        {
            desc.priceText = "<color=red>Cannot be sold</color>";
        }
        else if (price > 0)
        {
            desc.priceText = $"Price: {price} gold";
        }

        return desc;
    }

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
            case ItemType.Ring: return "Ring";
            default: return "Item";
        }
    }

    public virtual string GetInfoText() => GetDetailedInfo().mainStats;
}