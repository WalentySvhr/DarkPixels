using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent; // Сюди перетягни об'єкт Grid з Hierarchy
    public InventoryManager inventory; // Сюди перетягни об'єкт, де лежить InventoryManager (зазвичай Player)

    InventorySlot[] slots; // Масив усіх слотів у сітці

    void Start()
    {
        // Знаходимо всі скрипти слотів, які є всередині Grid
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        // Підписуємося на оновлення (якщо хочеш динамічно, але поки зробимо просто метод)
        UpdateUI();
    }

    // Метод, який проходить по списку предметів і малює їх у слоти
    public void UpdateUI()
    {
        // Знаходимо всі слоти, які є дітьми Grid
        InventorySlot[] slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // ПЕРЕДАЄМО ДАНІ ЗІ СТАКУ
                slots[i].AddItem(inventory.items[i].item, inventory.items[i].amount);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}