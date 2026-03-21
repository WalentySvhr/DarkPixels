using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [System.Serializable]
    public class Loot {
        public string name;
        public GameObject prefab;
        [Range(0f, 100f)]
        public float dropChance;
        
        [Header("Quantity")]
        public int minAmount = 1; // Мінімальна кількість
        public int maxAmount = 1; // Максимальна кількість
    }

    public Loot[] possibleLoot;

    public void DropLoot()
    {
        foreach (Loot item in possibleLoot)
        {
            if (item.prefab == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= item.dropChance)
            {
                // Визначаємо кількість для цього конкретного предмета
                int amountToDrop = Random.Range(item.minAmount, item.maxAmount + 1);

                // Створюємо вказану кількість предметів
                for (int i = 0; i < amountToDrop; i++)
                {
                    // Додаємо невеликий випадковий зсув (offset), щоб предмети не злипалися в одній точці
                    Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                    Instantiate(item.prefab, transform.position + randomOffset, Quaternion.identity);
                }
            }
        }
    }
}