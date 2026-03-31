using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Додаємо синглтон для легкого доступу

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;
    public InventoryUI inventoryUI;

    [Header("Гроші")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts; // Масив для текстів (в інвентарі та магазині)

    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        public int amount;
        public ItemStack(Item newItem, int newAmount) { item = newItem; amount = newAmount; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateCoinUI();
    }

    // МЕТОД ДОДАВАННЯ ПРЕДМЕТА
    public bool Add(Item item)
    {
        if (item.isStackable)
        {
            foreach (ItemStack stack in items)
            {
                if (stack.item == item && stack.amount < item.maxStackSize)
                {
                    stack.amount++;
                    UpdateUI();
                    return true;
                }
            }
        }

        if (items.Count >= space) return false;

        items.Add(new ItemStack(item, 1));
        UpdateUI();
        return true;
    }

    // МЕТОД ВИДАЛЕННЯ ПРЕДМЕТА
    public void Remove(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);

        if (stack != null)
        {
            stack.amount--;

            if (stack.amount <= 0)
            {
                items.Remove(stack);
            }

            UpdateUI();
        }
    }

    // ЛОГІКА МОНЕТ
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
            if (textElement != null)
            {
                textElement.text = "Золото: " + coins;
            }
        }
    }

    // ВИКОРИСТАННЯ ПРЕДМЕТА (Хілки тощо)
    public void UseItemFromHotbar(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);

        if (stack != null)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.Heal(item.healValue);
                stack.amount--;

                if (stack.amount <= 0)
                {
                    items.Remove(stack);
                }

                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI();

        // Якщо магазин відкритий під час змін в інвентарі, його теж треба оновити
        if (FisherShopManager.Instance != null && FisherShopManager.Instance.gameObject.activeInHierarchy)
        {
            FisherShopManager.Instance.RefreshShop();
        }
    }
}