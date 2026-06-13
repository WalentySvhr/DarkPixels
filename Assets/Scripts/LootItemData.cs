using UnityEngine;

[CreateAssetMenu(fileName = "NewLootItem", menuName = "Loot System/Loot Item")]
public class LootItemData : ScriptableObject
{
    public string itemName;
    public GameObject prefab;

    [Header("Налаштування кількості при дропі")]
    [Tooltip("Мінімальна кількість предметів, яка може спавнитися за один раз")]
    public int minAmount = 1;

    [Tooltip("Максимальна кількість предметів, яка може спавнитися за один раз")]
    public int maxAmount = 1;

    /// <summary>
    /// Повертає випадкову кількість предметів у заданому діапазоні.
    /// </summary>
    public int GetRandomAmount()
    {
        return Random.Range(minAmount, maxAmount + 1);
    }
}