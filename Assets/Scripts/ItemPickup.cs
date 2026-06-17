using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Сюди в Інспекторі ти зможеш перетягнути WeaponData (лук, меч) або зілля!
    public Item item;

    // Прапорець: чи активний предмет для підбору?
    private bool canPickup = false;

    void Start()
    {
        // Запускаємо таймер на 0.8 секунд. 
        // Зміни цю цифру, якщо хочеш, щоб предмет лежав "недоторканим" довше або менше.
        Invoke(nameof(EnablePickup), 1.1f);
    }

    void EnablePickup()
    {
        canPickup = true; // Тепер предмет можна підібрати!
    }

    // Використовуємо Stay замість Enter, щоб підібрати предмет, навіть якщо гравець стоїть на ньому
    void OnTriggerStay2D(Collider2D other)
    {
        // Якщо таймер ще не вийшов — скрипт просто ігнорує гравця
        if (!canPickup) return;

        if (other.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                bool pickedUp = InventoryManager.Instance.Add(item);

                // Якщо предмет додався в рюкзак (вистачило місця)
                if (pickedUp)
                {
                    // Вимикаємо колайдер, щоб інвентар не спробував підібрати його двічі 
                    // в ту саму мілісекунду, поки об'єкт ще не знищився
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    Destroy(gameObject);
                }
            }
        }
    }
}