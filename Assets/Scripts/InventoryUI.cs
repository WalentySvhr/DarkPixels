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

        List<InventoryManager.ItemStack> itemsToDisplay = new List<InventoryManager.ItemStack>();

        // 1. Створюємо список всіх ОДЯГНЕНИХ предметів
        // Якщо одягнено два однакових кільця, ця назва буде в списку двічі
        List<string> equippedNames = new List<string>();

        if (!string.IsNullOrEmpty(inventory.currentWeaponName)) equippedNames.Add(inventory.currentWeaponName);
        if (!string.IsNullOrEmpty(inventory.currentAmuletName)) equippedNames.Add(inventory.currentAmuletName);
        if (!string.IsNullOrEmpty(inventory.currentRing1Name)) equippedNames.Add(inventory.currentRing1Name);
        if (!string.IsNullOrEmpty(inventory.currentRing2Name)) equippedNames.Add(inventory.currentRing2Name);

        // 2. Фільтруємо сумку
        foreach (var stack in inventory.items)
        {
            if (stack != null && stack.item != null && stack.amount > 0)
            {
                bool hideThisItem = false;

                // Якщо назва цього предмета є в списку "одягнених"
                if (equippedNames.Contains(stack.item.name))
                {
                    hideThisItem = true;
                    // ВИКРЕСЛЮЄМО одну копію зі списку. 
                    // Наступне кільце з такою ж назвою вже не сховається!
                    equippedNames.Remove(stack.item.name);
                }

                // Додаємо в сумку ТІЛЬКИ якщо ми не вирішили його сховати
                if (!hideThisItem)
                {
                    itemsToDisplay.Add(stack);
                }
            }
        }

        // 3. Малюємо відфільтровані предмети у слоти сумки
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