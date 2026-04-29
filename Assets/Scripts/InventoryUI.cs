using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Обов'язкові посилання")]
    public Transform itemsParent; // Об'єкт Grid (сітка інвентарю)
    public InventoryManager inventory; // Посилання на InventoryManager

    private InventorySlot[] slots; // Список слотів у сітці

    void Awake()
    {
        // Знаходимо слоти один раз при старті
        RefreshSlots();
    }

    void Start()
    {
        UpdateUI();
    }

    public void RefreshSlots()
    {
        if (itemsParent != null)
        {
            slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);
        }
    }

    public void UpdateUI()
    {
        if (inventory == null) return;
        if (slots == null || slots.Length == 0) RefreshSlots();

        // 1. Створюємо список предметів, які НЕ екіпіровані
        List<InventoryManager.ItemStack> itemsToDisplay = new List<InventoryManager.ItemStack>();

        foreach (var stack in inventory.items)
        {
            if (stack != null && stack.item != null && stack.amount > 0)
            {
                // Перевіряємо, чи цей предмет зараз в руках або на шиї
                bool isEquipped = (stack.item.name == inventory.currentWeaponName ||
                                   stack.item.name == inventory.currentAmuletName);

                // Додаємо в сумку ТІЛЬКИ якщо предмет не екіпірований
                if (!isEquipped)
                {
                    itemsToDisplay.Add(stack);
                }
            }
        }

        // 2. Малюємо відфільтровані предмети у слоти сумки
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < itemsToDisplay.Count)
            {
                slots[i].AddItem(itemsToDisplay[i].item, itemsToDisplay[i].amount);
            }
            else
            {
                // Очищуємо порожні слоти сумки
                slots[i].ClearSlot();
            }
        }
    }

    // Автоматично оновлюємо UI при відкритті вікна інвентарю
    void OnEnable()
    {
        UpdateUI();
    }
}