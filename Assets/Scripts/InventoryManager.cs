using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;

    [Header("UI посилання")]
    public InventoryUI inventoryUI;
    public HotbarUI hotbarScript;

    [Header("Поточна екіпіровка (Універсальна система)")]
    // --- НОВЕ: Словник замість десятка окремих змінних ---
    // Ключ: "Weapon", "Amulet", "Ring_1", "Belt" і т.д.
    // Значення: Назва предмета
    public Dictionary<string, string> equippedItems = new Dictionary<string, string>();

    [Header("Гроші")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts;

    [Header("Система Бафів")]
    public float coinMultiplier = 1f;
    private Coroutine activeBoostCoroutine;

    [Header("UI Бафів (Таймер)")]
    public GameObject buffUIContainer;
    public TextMeshProUGUI buffTimerText;

    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        public int amount;
        public ItemStack(Item newItem, int newAmount) { item = newItem; amount = newAmount; }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateCoinUI();
        UpdateUI();
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
    }

    public bool Add(Item item)
    {
        if (item.isStackable)
        {
            ItemStack stack = items.Find(s => s.item == item && s.amount < item.maxStackSize);
            if (stack != null)
            {
                stack.amount++;
                UpdateUI();
                if (QuestManager.Instance != null) QuestManager.Instance.UpdateCollectItemProgress();
                return true;
            }
        }

        if (items.Count >= space) return false;

        items.Add(new ItemStack(item, 1));
        UpdateUI();

        if (QuestManager.Instance != null) QuestManager.Instance.UpdateCollectItemProgress();
        return true;
    }

    public void Remove(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);
        if (stack != null)
        {
            stack.amount--;
            if (stack.amount <= 0) items.Remove(stack);
            UpdateUI();
        }
    }

    public int GetItemCount(Item itemToFind)
    {
        int count = 0;
        foreach (ItemStack stack in items)
        {
            if (stack.item == itemToFind) count += stack.amount;
        }
        return count;
    }

    public void RemoveItems(Item itemToRemove, int amountToRemove)
    {
        int remainingToRemove = amountToRemove;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            ItemStack stack = items[i];
            if (stack.item == itemToRemove)
            {
                if (stack.amount >= remainingToRemove)
                {
                    stack.amount -= remainingToRemove;
                    remainingToRemove = 0;
                    if (stack.amount <= 0) items.RemoveAt(i);
                    break;
                }
                else
                {
                    remainingToRemove -= stack.amount;
                    items.RemoveAt(i);
                }
            }
        }
        UpdateUI();
    }

    // === МАГІЯ УНІВЕРСАЛЬНОСТІ ТУТ ===
    // Цей метод тепер проковтне БУДЬ-ЯКИЙ слот (Belt, Helmet, Boots, Ring 1, Ring 2...)
    public void EquipItem(Item item, string slotType, int slotIndex = 0)
    {
        // Якщо це кільце 1, ключ буде "Ring_1". Якщо звичайна зброя - ключ "Weapon"
        string slotKey = slotIndex > 0 ? $"{slotType}_{slotIndex}" : slotType;

        // Просто записуємо в словник
        equippedItems[slotKey] = item.name;

        Debug.Log($"[InventoryManager] В слот {slotKey} записано {item.name}");
    }

    public void UnequipItem(string slotType, int slotIndex = 0)
    {
        string slotKey = slotIndex > 0 ? $"{slotType}_{slotIndex}" : slotType;

        // Видаляємо запис зі словника, якщо він там був
        if (equippedItems.ContainsKey(slotKey))
        {
            equippedItems.Remove(slotKey);
            Debug.Log($"[InventoryManager] Зі слота {slotKey} знято предмет");
        }
    }
    // ===================================

    public void ChangeCoins(int amount)
    {
        coins += amount;
        if (coins < 0) coins = 0;
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        if (coinTexts == null) return;
        foreach (TextMeshProUGUI textElement in coinTexts)
        {
            if (textElement != null) textElement.text = " " + coins;
        }
    }

    public void UseItem(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);
        if (stack == null) return;

        if (item.healValue > 0)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.Heal(item.healValue);
                stack.amount--;
                if (stack.amount <= 0) items.Remove(stack);
                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI();
        if (hotbarScript != null) hotbarScript.UpdateHotbar();

        if (ShopManager.Instance != null && ShopManager.Instance.gameObject.activeInHierarchy)
        {
            try { ShopManager.Instance.RefreshShop(); } catch (System.Exception) { }
        }

        if (StatsUI.Instance != null && StatsUI.Instance.gameObject.activeInHierarchy)
        {
            StatsUI.Instance.UpdateStatsUI();
        }
    }

    public void ActivateCoinBoost(float multiplier, float durationInSeconds)
    {
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);
        activeBoostCoroutine = StartCoroutine(CoinBoostRoutine(multiplier, durationInSeconds));
    }

    private System.Collections.IEnumerator CoinBoostRoutine(float multiplier, float duration)
    {
        coinMultiplier = multiplier;
        float timeRemaining = duration;

        if (buffUIContainer != null) buffUIContainer.SetActive(true);

        while (timeRemaining > 0)
        {
            if (buffTimerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                buffTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        coinMultiplier = 1f;
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
    }

    public void AddMobCoins(int baseAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseAmount * coinMultiplier);
        ChangeCoins(finalAmount);
    }
}