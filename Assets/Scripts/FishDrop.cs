using UnityEngine;
using System.Collections.Generic;
public enum FishType { Common, Rare, Golden, Junk }

[System.Serializable]
public class FishLootData
{
    public string name;
    public FishType type;
    public Item itemData; // Змінили GameObject на Item
    [Range(0, 100)]
    public float dropChance;
}

public class FishDrop : MonoBehaviour
{
    public List<FishLootData> lootTable = new List<FishLootData>();

    public Item GetRandomFishItem() // Повертає об'єкт типу Item
    {
        float roll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        foreach (var item in lootTable)
        {
            cumulativeChance += item.dropChance;
            if (roll <= cumulativeChance) return item.itemData;
        }
        return null;
    }
}