using UnityEngine;

// Додаємо категорії, щоб було легше фільтрувати предмети
public enum ItemType { Food, Potion, Resource, Junk }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Загальна інформація")]
    public string itemName;
    public ItemType type; // Вибираємо тип у випадаючому списку
    public Sprite icon;
    public int price;     // Ціна для продажу торговцю

    [Header("Налаштування стаку")]
    public bool isStackable;
    public int maxStackSize = 10;

    [Header("Ефекти (якщо є)")]
    public int healValue = 0; // Для риби буде 0, для зілля — наприклад, 20
}