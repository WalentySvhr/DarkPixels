using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;
    public InventoryUI inventoryUI;

    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        public int amount;
        public ItemStack(Item newItem, int newAmount) { item = newItem; amount = newAmount; }
    }

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
    public void UseItemFromHotbar(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);

        if (stack != null)
        {
            // Звертаємося до твого скрипта PlayerHealth
            PlayerHealth health = GetComponent<PlayerHealth>();

            if (health != null)
            {
                // Викликаємо метод Heal, який ти написав
                // Можна додати в клас Item змінну healAmount, щоб різні зілля лікували по-різному
                health.Heal(item.healValue);

                stack.amount--;

                if (stack.amount <= 0)
                {
                    items.Remove(stack);
                }

                if (inventoryUI != null) inventoryUI.UpdateUI();
            }
        }
    }

    void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI();
    }
}