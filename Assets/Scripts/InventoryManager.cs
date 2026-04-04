using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Синглтон

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;
    public InventoryUI inventoryUI;

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

    // УНІВЕРСАЛЬНИЙ МЕТОД ВИКОРИСТАННЯ ПРЕДМЕТА (Тепер тільки клік по зіллю)
    public void UseItem(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);
        if (stack == null) return;

        // ЯКЩО ЦЕ ЗІЛЛЯ (Heal)
        if (item.healValue > 0)
        {
            // Найнадійніший спосіб знайти здоров'я гравця, де б не висів Інвентар
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();

            if (health != null)
            {
                health.Heal(item.healValue); // Лікуємо гравця

                stack.amount--; // Забираємо 1 зілля

                if (stack.amount <= 0)
                {
                    items.Remove(stack); // Якщо закінчилися - прибираємо іконку
                }

                UpdateUI(); // Оновлюємо інвентар
                Debug.Log("Випито зілля! Відновлено ХП: " + item.healValue);
            }
            else
            {
                Debug.LogWarning("Помилка: Не знайдено скрипт PlayerHealth на сцені!");
            }
        }
        else if (item is WeaponData)
        {
            // Якщо випадково клікнули по зброї, підказуємо в консоль, що її треба тягнути
            Debug.Log("Зброю потрібно перетягнути у слот екіпіровки!");
        }
        else
        {
            // Якщо це не зброя і healValue = 0, виводимо підказку
            Debug.Log("Цей предмет не можна використати таким чином!");
        }
    }

    // ОНОВЛЕННЯ ІНТЕРФЕЙСУ
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