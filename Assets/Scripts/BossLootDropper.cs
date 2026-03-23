using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossLoot
{
    public string name;
    public GameObject itemPrefab;
    [Range(0, 100)] public float dropChance;
    public int minAmount = 1;
    public int maxAmount = 1;
}

public class BossLootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    public List<BossLoot> lootTable;

    [Header("Visual Effects")]
    public GameObject victoryEffect;

    // Створюємо посилання на наш новий скрипт фізики
    private LootPhysics lootPhysics;

    void Awake()
    {
        lootPhysics = GetComponent<LootPhysics>();
        // Якщо забув додати компонент в інспекторі, додаємо його самі
        if (lootPhysics == null) lootPhysics = gameObject.AddComponent<LootPhysics>();
    }

    public void DropBossLoot()
    {
        if (victoryEffect != null)
        {
            Instantiate(victoryEffect, transform.position, Quaternion.identity);
        }

        foreach (BossLoot loot in lootTable)
        {
            float roll = Random.Range(0f, 100f);

            if (roll <= loot.dropChance)
            {
                int count = Random.Range(loot.minAmount, loot.maxAmount + 1);

                for (int i = 0; i < count; i++)
                {
                    // 1. Просто спавнимо префаб
                    GameObject droppedItem = Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);

                    // 2. Віддаємо його скрипту фізики для розльоту
                    lootPhysics.ApplyExplosion(droppedItem);
                }
            }
        }
    }
}