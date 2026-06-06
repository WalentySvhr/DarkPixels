using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Префаби та Контейнери")]
    public GameObject slotPrefab;
    public Transform traderNPCContent;
    public Transform traderPlayerContent;
    public GameObject shopPanel;

    public InventoryManager playerInv;

    public ShopData currentShop;

    // Змінено з NPCPatrol на MonoBehaviour, щоб уникнути помилок після видалення класу
    private MonoBehaviour currentNPC;

    void Awake() => Instance = this;

    // Метод тепер приймає MonoBehaviour (будь-який скрипт на об'єкті)
    public void OpenShop(ShopData shop, MonoBehaviour npc = null)
    {
        Debug.Log($"[Магазин] Відкриваємо магазин! Передано файл: {(shop != null ? shop.shopName : "ПУСТИЙ ФАЙЛ")}");
        currentShop = shop;
        currentNPC = npc;

        // ВМИКАЄМО ЗАПОБІЖНИК: Кажемо грі, що вікно відкрито
        UIManager.RegisterWindowOpen();

        shopPanel.SetActive(true);
        RefreshShop();
    }

    public void CloseShop()
    {
        // 1. Очищаємо дані
        currentShop = null;

        // 2. Вимикаємо панель
        shopPanel.SetActive(false);

        // ВИМИКАЄМО ЗАПОБІЖНИК: Кажемо грі, що вікон більше немає
        UIManager.RegisterWindowClose();

        // 3. РОЗБЛОКУВАННЯ: Скидаємо виділення UI
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 4. Якщо гра була на паузі
        Time.timeScale = 1f;

        // 5. Логіка NPC
        if (currentNPC != null)
        {
            currentNPC = null;
        }

        Debug.Log("Магазин закрито, управління розблоковано.");
    }

    public void RefreshShop()
    {
        if (currentShop == null) return;

        UpdateGridNPC(traderNPCContent, currentShop.itemsForSale);

        if (playerInv != null)
        {
            UpdateGridPlayer(traderPlayerContent, playerInv.items);
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
                slotScript.Setup(stack.item, true, stack.amount);
        }
    }

    public void BuyItem(Item item)
    {
        if (playerInv.coins >= item.price)
        {
            if (playerInv.Add(item))
            {
                playerInv.ChangeCoins(-item.price);
                if (DailyQuestManager.Instance != null)
                {
                    DailyQuestManager.Instance.AddProgress(DailyQuestType.SpendGold, item.price);
                }
                RefreshShop();
            }
        }
    }

    public void SellItem(Item item)
    {
        int sellPrice = Mathf.RoundToInt(item.price * currentShop.sellMultiplier);
        if (sellPrice < 1) sellPrice = 1;

        playerInv.Remove(item);
        playerInv.ChangeCoins(sellPrice);

        if (item.type == ItemType.Resource)
        {
            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.AddProgress(DailyQuestType.SellResources, sellPrice);
            }
        }
        RefreshShop();
    }
}