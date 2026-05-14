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

        // Оскільки новий InventoryManager ФІЗИЧНО забирає екіпіровані предмети 
        // зі списку inventory.items, нам більше не потрібна складна фільтрація!

        // Просто беремо те, що є в сумці, і малюємо в слотах:
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // Якщо для цього слота є предмет у списку інвентарю - показуємо його
                slots[i].AddItem(inventory.items[i].item, inventory.items[i].amount);
            }
            else
            {
                // Всі інші слоти очищуємо
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