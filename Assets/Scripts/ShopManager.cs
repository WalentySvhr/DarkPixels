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

    [Header("Налаштування рамок")]
    public int maxTraderSlots = 12; // Скільки всього рамок показувати у NPC
    public int maxPlayerSlots = 12; // Скільки всього рамок показувати у Гравця

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

        // Цикл тепер працює до maxTraderSlots, створюючи рамки завжди
        for (int i = 0; i < maxTraderSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, container);
            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();

            if (slotScript != null)
            {
                // Перевіряємо, чи є товар для цього індексу
                if (i < items.Count && items[i] != null)
                {
                    slotScript.Setup(items[i], false); // Звичайне налаштування з товаром
                }
                else
                {
                    slotScript.Setup(null, false); // Передаємо null, бо слот порожній
                }
            }
        }
    }

    private void UpdateGridPlayer(Transform container, List<InventoryManager.ItemStack> stacks)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        // Цикл працює до maxPlayerSlots
        for (int i = 0; i < maxPlayerSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, container);
            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();

            if (slotScript != null)
            {
                // Перевіряємо, чи є стек предметів для цього індексу
                if (i < stacks.Count && stacks[i] != null && stacks[i].item != null)
                {
                    slotScript.Setup(stacks[i].item, true, stacks[i].amount);
                }
                else
                {
                    slotScript.Setup(null, true); // Передаємо null для порожнього слота
                }
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