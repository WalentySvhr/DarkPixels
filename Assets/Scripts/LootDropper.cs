using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Таблиця луту")]
    public LootTable lootTable;

    [Header("Кількість спроб дропу")]
    [Range(1, 5)] public int dropRolls = 1;

    [Header("Зникнення предметів (Оптимізація)")]
    public bool destroyItemsAfterTime = true;
    public float itemLifetime = 15f;

    private LootPhysics lootPhysics;
    private SpriteRenderer enemySprite;

    void Awake()
    {
        lootPhysics = GetComponent<LootPhysics>();
        if (lootPhysics == null) lootPhysics = gameObject.AddComponent<LootPhysics>();
        enemySprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void DropLoot()
    {
        if (lootTable == null) return;

        string currentLayer = (enemySprite != null) ? enemySprite.sortingLayerName : "Default";
        Transform targetContainer = (TowerManager.Instance != null) ? TowerManager.Instance.lootContainer : null;

        for (int roll = 0; roll < dropRolls; roll++)
        {
            LootItemData droppedItemData = lootTable.GetRandomItem();

            if (droppedItemData == null || droppedItemData.prefab == null) continue;

            // --- МОДИФІКОВАНО ---
            // Тепер кількість береться індивідуально з самого Scriptable Object предмета
            int amountToDrop = droppedItemData.GetRandomAmount();

            for (int i = 0; i < amountToDrop; i++)
            {
                GameObject droppedItem;
                if (targetContainer != null)
                {
                    droppedItem = Instantiate(droppedItemData.prefab, transform.position, Quaternion.identity, targetContainer);
                }
                else
                {
                    droppedItem = Instantiate(droppedItemData.prefab, transform.position, Quaternion.identity);
                }

                ApplyLighting(droppedItem, currentLayer);
                lootPhysics.ApplyExplosion(droppedItem);

                if (destroyItemsAfterTime)
                {
                    Destroy(droppedItem, itemLifetime);
                }
            }
        }
    }

    private void ApplyLighting(GameObject itemObj, string targetLayer)
    {
        SpriteRenderer[] sprites = itemObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer s in sprites) s.sortingLayerName = targetLayer;

        Canvas[] canvases = itemObj.GetComponentsInChildren<Canvas>();
        foreach (Canvas c in canvases) c.sortingLayerName = targetLayer;
    }
}