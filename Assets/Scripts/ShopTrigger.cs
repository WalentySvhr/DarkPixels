using UnityEngine;

// Видалили using UnityEngine.EventSystems; оскільки він більше не потрібен

[RequireComponent(typeof(Collider2D))]
public class ShopTrigger : MonoBehaviour
{
    [Header("Файл Магазину")]
    public ShopData myShopData;

    [Header("Налаштування")]
    public float interactionRadius = 2.0f;

    private bool playerInRange = false;

    private void Update()
    {
        playerInRange = IsPlayerNearby();
    }

    private bool IsPlayerNearby()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        return Vector2.Distance(transform.position, player.transform.position) <= interactionRadius;
    }

    private void OnMouseDown()
    {
        // Перевірку EventSystem видалено

        Debug.Log("Клік по торговцю: " + gameObject.name);

        if (playerInRange)
        {
            OpenMyShop();
        }
        else
        {
            Debug.Log("Гравець занадто далеко!");
        }
    }

    public void OpenMyShop()
    {
        if (ShopManager.Instance == null)
        {
            Debug.LogError("ShopManager не знайдено на сцені!");
            return;
        }

        if (myShopData == null)
        {
            Debug.LogError($"myShopData не призначено на {gameObject.name}!");
            return;
        }

        Debug.Log($"Відкриваємо магазин: {myShopData.shopName}");
        ShopManager.Instance.OpenShop(myShopData, this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}