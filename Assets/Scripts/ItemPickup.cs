using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // Який саме предмет лежить на землі

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bool pickedUp = other.GetComponent<InventoryManager>().Add(item);
            if (pickedUp) Destroy(gameObject); // Видаляємо предмет зі сцени
        }
    }
}