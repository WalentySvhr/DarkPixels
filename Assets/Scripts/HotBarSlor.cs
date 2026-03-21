using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public Item itemToTrack; // Сюди перетягни ассет (ScriptableObject) банки
    public InventorySlot slotUI; // Сюди перетягни цей же об'єкт (сам слот)
    
    private InventoryManager inventory;

    void Start()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
    }

    void Update()
    {
        UpdateHotbar();
    }

    void UpdateHotbar()
    {
        if (inventory == null || itemToTrack == null) return;

        // Шукаємо, чи є таке зілля в інвентарі
        InventoryManager.ItemStack foundStack = inventory.items.Find(s => s.item == itemToTrack);

        if (foundStack != null)
        {
            // Якщо знайшли — показуємо іконку та кількість
            slotUI.AddItem(foundStack.item, foundStack.amount);
        }
        else
        {
            // Якщо банок 0 — очищуємо слот
            slotUI.ClearSlot();
        }
    }
}