using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [System.Serializable]
    public class Loot
    {
        public string name;
        public GameObject prefab;
        [Range(0f, 100f)] public float dropChance;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    public Loot[] possibleLoot;
    private LootPhysics lootPhysics;

    void Awake()
    {
        // Шукаємо компонент фізики на цьому ж об'єкті
        lootPhysics = GetComponent<LootPhysics>();

        // Якщо забули додати в інспекторі — додаємо автоматично
        if (lootPhysics == null) lootPhysics = gameObject.AddComponent<LootPhysics>();
    }

    public void DropLoot()
    {
        foreach (Loot item in possibleLoot)
        {
            if (item.prefab == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= item.dropChance)
            {
                int amountToDrop = Random.Range(item.minAmount, item.maxAmount + 1);

                for (int i = 0; i < amountToDrop; i++)
                {
                    // Спавнимо об'єкт
                    GameObject droppedItem = Instantiate(item.prefab, transform.position, Quaternion.identity);

                    // Передаємо його скрипту фізики для розльоту
                    lootPhysics.ApplyExplosion(droppedItem);
                }
            }
        }
    }
}