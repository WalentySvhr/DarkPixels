using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Префаби та Контейнери")]
    public GameObject slotPrefab;
    public Transform traderNPCContent;
    public Transform traderPlayerContent;
    public GameObject shopPanel; // Сама панелька магазину, щоб її вмикати/вимикати

    public InventoryManager playerInv;

    // ВИПРАВЛЕНО: Тепер public, щоб FisherShopSlot міг дізнатися множник цін!
    public ShopData currentShop;

    void Awake() => Instance = this;

    // ВИКЛИКАЄТЬСЯ З ТРИГЕРА NPC (наприклад, коли клікнув на торговця)
    public void OpenShop(ShopData shop)
    {
        Debug.Log($"[Магазин] Відкриваємо магазин! Передано файл: {(shop != null ? shop.shopName : "ПУСТИЙ ФАЙЛ")}");
        currentShop = shop;
        shopPanel.SetActive(true);
        RefreshShop();
    }

    public void CloseShop()
    {
        currentShop = null;
        shopPanel.SetActive(false);
    }

    public void RefreshShop()
    {
        if (currentShop == null)
        {

            return;
        }

        Debug.Log($"[Магазин] Оновлюємо вітрину NPC. Знайдено товарів у файлі: {currentShop.itemsForSale.Count}");
        UpdateGridNPC(traderNPCContent, currentShop.itemsForSale);

        if (playerInv != null)
        {
            Debug.Log($"[Магазин] Оновлюємо вітрину Гравця. Знайдено речей в інвентарі: {playerInv.items.Count}");
            UpdateGridPlayer(traderPlayerContent, playerInv.items);
        }
        else
        {
            Debug.LogWarning("[Магазин] Увага: Не підключено інвентар гравця (Player Inv) в Інспекторі!");
        }
    }

    private void UpdateGridNPC(Transform container, List<Item> items)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (Item item in items)
        {
            if (item == null) continue;
            GameObject obj = Instantiate(slotPrefab, container);

            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();
            if (slotScript != null)
            {
                slotScript.Setup(item, false);
            }
        }
    }

    private void UpdateGridPlayer(Transform container, List<InventoryManager.ItemStack> stacks)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var stack in stacks)
        {
            if (stack == null || stack.item == null) continue;
            GameObject obj = Instantiate(slotPrefab, container);

            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();
            if (slotScript != null)
            {
                // Передаємо amount
                slotScript.Setup(stack.item, true, stack.amount);
            }
        }
    }

    public void BuyItem(Item item)
    {
        if (playerInv.coins >= item.price)
        {
            if (playerInv.Add(item))
            {
                playerInv.ChangeCoins(-item.price);

                // --- НОВЕ: Звіт для дейліків (витрата золота) ---
                if (DailyQuestManager.Instance != null)
                {
                    DailyQuestManager.Instance.AddProgress(DailyQuestType.SpendGold, item.price);
                }
                // ------------------------------------------------

                RefreshShop();
            }
            else
            {
                Debug.Log("Немає місця в інвентарі!");
            }
        }
        else
        {
            Debug.Log("Недостатньо монет!");
        }
    }

    public void SellItem(Item item)
    {
        // Формула уцінки: 100 монет * 0.5 (multiplier) = 50 монет
        int sellPrice = Mathf.RoundToInt(item.price * currentShop.sellMultiplier);

        // Щоб речі не коштували 0 монет, ставимо мінімум 1
        if (sellPrice < 1) sellPrice = 1;

        playerInv.Remove(item);
        playerInv.ChangeCoins(sellPrice);

        // --- ВИПРАВЛЕНО: Фільтруємо предмети перед звітом у дейліки ---
        // Зараховуємо прогрес ТІЛЬКИ якщо тип предмета - Resource
        if (item.type == ItemType.Resource)
        {
            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.AddProgress(DailyQuestType.SellResources, sellPrice);
            }
        }
        // -------------------------------------------------

        RefreshShop();
    }
}