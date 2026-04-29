using UnityEngine;
using System.Collections.Generic;

public class HotbarUI : MonoBehaviour
{
    public Transform hotbarParent;
    public InventoryManager inventory;

    private InventorySlot[] hotbarSlots;
    void Start()
    {
        // Оновлюємо хотбар при старті гри
        UpdateHotbar();
    }

    // Також корисно додати це, щоб хотбар оновлювався, коли панель стає активною
    void OnEnable()
    {
        UpdateHotbar();
    }
    void Awake()
    {
        if (hotbarParent != null)
        {
            hotbarSlots = hotbarParent.GetComponentsInChildren<InventorySlot>(true);
            foreach (var slot in hotbarSlots)
            {
                slot.isHotbarSlot = true;
            }
        }
    }

    public void UpdateHotbar()
    {
        if (inventory == null || hotbarSlots == null) return;

        // Створюємо список спеціально для зіллів
        List<InventoryManager.ItemStack> potionStacks = new List<InventoryManager.ItemStack>();

        foreach (var stack in inventory.items)
        {
            if (stack != null && stack.item != null)
            {
                // УМОВА: Перевіряємо саме поле 'type' зі скрипта Item
                // Тепер риба (ItemType.Food) сюди не потрапить, навіть якщо лікує
                if (stack.item.type == ItemType.Potion && stack.item.healValue > 0)
                {
                    potionStacks.Add(stack);
                }
            }
        }

        // Заповнюємо слоти хотбару знайденими стаками
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i < potionStacks.Count)
            {
                hotbarSlots[i].AddItem(potionStacks[i].item, potionStacks[i].amount);
            }
            else
            {
                // Якщо зілля закінчилися або їх менше ніж слотів - чистимо
                hotbarSlots[i].ClearSlot();
            }
        }
    }
}