using UnityEngine;
using UnityEngine.EventSystems; // Потрібно для блокування кліків через UI

public class CurrencyExchangeNPC : MonoBehaviour
{
    [Header("Налаштування взаємодії")]
    public float interactRange = 2.5f; // Дистанція для тапу

    private Transform playerTransform;

    void Start()
    {
        // Шукаємо гравця за тегом (як і в інших торговцях)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Відловлюємо тап по NPC на мобілці
    private void OnMouseDown()
    {
        // Захист: якщо клік по відкритому UI (інвентар, налаштування) — ігноруємо тап!
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (playerTransform == null) return;

        // Перевіряємо відстань
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactRange)
        {
            OpenExchangeWindow();
        }
        else
        {
            Debug.Log("Підійдіть ближче до Міняйла!");
            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNotification("Підійдіть ближче до Міняйла!");
            }
        }
    }

    public void OpenExchangeWindow()
    {
        // Захист від помилок і випадкового подвійного тапу
        if (CurrencyExchangeUI.Instance == null) return;
        if (CurrencyExchangeUI.Instance.windowPanel != null && CurrencyExchangeUI.Instance.windowPanel.activeInHierarchy) return;

        // Відкриваємо наше вікно обміну
        CurrencyExchangeUI.Instance.Open();

        // Якщо цей NPC теж вміє ходити і використовує NPCPatrol, зупиняємо його:
        NPCPatrol patrol = GetComponent<NPCPatrol>();
        if (patrol != null) patrol.StartInteraction();
    }
}