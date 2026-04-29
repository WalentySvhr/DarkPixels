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

    [Header("Налаштування випадіння")]
    public Loot[] possibleLoot;

    [Header("Зникнення предметів (Оптимізація)")]
    [Tooltip("Чи повинні предмети зникати з часом?")]
    public bool destroyItemsAfterTime = true;
    [Tooltip("Скільки секунд предмет лежатиме на землі перед зникненням")]
    public float itemLifetime = 30f;

    private LootPhysics lootPhysics;
    private SpriteRenderer enemySprite;

    void Awake()
    {
        lootPhysics = GetComponent<LootPhysics>();
        if (lootPhysics == null) lootPhysics = gameObject.AddComponent<LootPhysics>();

        // Запам'ятовуємо спрайт ворога, щоб знати, на якому він шарі (світлі)
        enemySprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void DropLoot()
    {
        // Дізнаємося поточний шар ворога (наприклад, "Enemy_Inside" або "Enemy_Outside")
        string currentLayer = (enemySprite != null) ? enemySprite.sortingLayerName : "Default";

        foreach (Loot item in possibleLoot)
        {
            if (item.prefab == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= item.dropChance)
            {
                int amountToDrop = Random.Range(item.minAmount, item.maxAmount + 1);

                for (int i = 0; i < amountToDrop; i++)
                {
                    GameObject droppedItem = Instantiate(item.prefab, transform.position, Quaternion.identity);

                    // Прив'язка до освітлення
                    ApplyLighting(droppedItem, currentLayer);

                    // Застосовуємо фізику (розліт)
                    lootPhysics.ApplyExplosion(droppedItem);

                    // --- НОВЕ: Таймер знищення ---
                    // Якщо галочка стоїть, кажемо Unity знищити предмет через itemLifetime секунд
                    if (destroyItemsAfterTime)
                    {
                        Destroy(droppedItem, itemLifetime);
                    }
                }
            }
        }
    }

    private void ApplyLighting(GameObject itemObj, string targetLayer)
    {
        // 1. Міняємо шар для всіх спрайтів предмета
        SpriteRenderer[] sprites = itemObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer s in sprites)
        {
            s.sortingLayerName = targetLayer;
        }

        // 2. Міняємо шар для Canvas (якщо у предмета є напис із назвою над ним)
        Canvas[] canvases = itemObj.GetComponentsInChildren<Canvas>();
        foreach (Canvas c in canvases)
        {
            c.sortingLayerName = targetLayer;
        }
    }
}