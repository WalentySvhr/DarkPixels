using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Сюди в Інспекторі ти зможеш перетягнути WeaponData (лук, меч) або зілля!
    public Item item;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Використовуємо наш Instance (Синглтон) замість GetComponent
            if (InventoryManager.Instance != null)
            {
                bool pickedUp = InventoryManager.Instance.Add(item);

                // Якщо предмет додався в рюкзак (вистачило місця) - видаляємо його з землі
                if (pickedUp)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}