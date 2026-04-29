using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("Файл Магазину")]
    [Tooltip("Перетягни сюди файл ShopData (наприклад, асортимент Коваля)")]
    public ShopData myShopData;

    [Header("Налаштування взаємодії")]
    public float interactRange = 2.5f; // Дистанція для тапу

    private Transform playerTransform;

    void Start()
    {
        // Шукаємо гравця за тегом (як і в діалогах)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Відловлюємо тап по NPC на мобілці
    private void OnMouseDown()
    {
        if (playerTransform == null) return;

        // Перевіряємо відстань
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactRange)
        {
            OpenMyShop();
        }
        else
        {
            Debug.Log("Підійдіть ближче до торговця!");
        }
    }

    public void OpenMyShop()
    {
        // Захист від помилок і випадкового подвійного тапу
        if (ShopManager.Instance == null) return;
        if (ShopManager.Instance.shopPanel != null && ShopManager.Instance.shopPanel.activeInHierarchy) return;

        if (myShopData != null)
        {
            // Передаємо нашому Менеджеру конкретні товари цього NPC
            ShopManager.Instance.OpenShop(myShopData);

            // Якщо треба зупинити рух торговця (якщо він ходить)
            NPCPatrol patrol = GetComponent<NPCPatrol>();
            if (patrol != null) patrol.StartInteraction();
        }
        else
        {
            Debug.LogWarning($"На NPC {gameObject.name} не призначено файл ShopData!");
        }
    }
}