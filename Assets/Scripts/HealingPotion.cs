using UnityEngine;

public class HealingPotion : MonoBehaviour
{
    public Item item; // Твій ScriptableObject "Healing Potion" з іконкою та назвою

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Шукаємо інвентар на гравці
            InventoryManager inventory = collision.GetComponent<InventoryManager>();

            if (inventory != null)
            {
                // 2. Намагаємося додати предмет в інвентар
                bool wasPickedUp = inventory.Add(item);

                // 3. Якщо в інвентарі було місце і предмет додано
                if (wasPickedUp)
                {
                    // Тільки тепер знищуємо об'єкт зі сцени
                    Destroy(gameObject);
                }
            }
        }
    }
}