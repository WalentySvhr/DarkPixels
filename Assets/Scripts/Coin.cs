using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1; // Скільки монет дає цей предмет

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                // ВИКЛИКАЄМО НАШУ НОВУ ФУНКЦІЮ ЗАМІСТЬ СТАРОЇ
                InventoryManager.Instance.AddMobCoins(coinValue);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("InventoryManager не знайдено на сцені!");
            }
        }
    }
}