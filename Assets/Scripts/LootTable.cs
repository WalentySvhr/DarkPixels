using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot System/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class DropChance
    {
        public LootItemData item; // Якщо null, то це "Нічого" (пустота)
        [Tooltip("Чим більша вага, тим вищий шанс випадіння предмета")]
        public int weight;
    }

    public List<DropChance> lootList = new List<DropChance>();

    /// <summary>
    /// Повертає випадковий предмет на основі вагових коефіцієнтів.
    /// </summary>
    public LootItemData GetRandomItem()
    {
        if (lootList == null || lootList.Count == 0) return null;

        // 1. Рахуємо загальну суму всіх ваг
        int totalWeight = 0;
        foreach (var drop in lootList)
        {
            totalWeight += drop.weight;
        }

        // 2. Вибираємо випадкове число в межах загальної ваги
        int rolledValue = Random.Range(0, totalWeight);

        // 3. Шукаємо, в який відрізок ваги потрапило число
        int currentWeightSum = 0;
        foreach (var drop in lootList)
        {
            currentWeightSum += drop.weight;
            if (rolledValue < currentWeightSum)
            {
                return drop.item; // Повертаємо вибраний предмет (може бути null)
            }
        }

        return null;
    }
}