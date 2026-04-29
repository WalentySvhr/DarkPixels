using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;

    [Header("UI посилання (Окремі)")]
    public InventoryUI inventoryUI; // Скрипт для основної сітки сумки
    public HotbarUI hotbarScript;   // НОВИЙ окремий скрипт для хотбару

    [Header("Поточна екіпіровка (Тільки назви)")]
    public string currentWeaponName;
    public string currentAmuletName;

    [Header("Гроші")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts;

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
        UpdateUI(); // Додай цей рядок, якщо його немає
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
                return true;
            }
        }

        if (items.Count >= space) return false;

        items.Add(new ItemStack(item, 1));
        UpdateUI();
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

    public void EquipItem(Item item, bool isWeapon)
    {
        if (isWeapon) currentWeaponName = item.name;
        else currentAmuletName = item.name;
    }

    public void UnequipItem(bool isWeapon)
    {
        if (isWeapon) currentWeaponName = "";
        else currentAmuletName = "";
    }

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

                // Після використання оновлюємо все
                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        // 1. Оновлюємо основну сумку
        if (inventoryUI != null) inventoryUI.UpdateUI();

        // 2. Оновлюємо хотбар через його власну логіку
        if (hotbarScript != null) hotbarScript.UpdateHotbar();

        if (ShopManager.Instance != null && ShopManager.Instance.gameObject.activeInHierarchy)
        {
            try { ShopManager.Instance.RefreshShop(); }
            catch (System.Exception) { }
        }
    }
}