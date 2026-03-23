using UnityEngine;

[System.Serializable]
public class LootItemChest
{
    public string itemName;      // Назва (для тебе)
    public GameObject itemPrefab; // Що саме спавнити (монетка, зілля, меч)
    [Range(0, 100)]
    public float dropChance;     // Шанс випадіння (0-100%)
    public int minAmount = 1;    // Мінімальна кількість
    public int maxAmount = 1;    // Максимальна кількість
}