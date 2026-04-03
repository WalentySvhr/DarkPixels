using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [Header("UI посилання")]
    public GameObject shopPanel;    // Сюди кидаєш будь-яку панель (зброя, риба, магія)
    public GameObject interactIcon; // Іконка над головою

    private bool isPlayerNearby = false;
    private NPCPatrol patrol;

    void Start()
    {
        patrol = GetComponent<NPCPatrol>();
        if (shopPanel != null) shopPanel.SetActive(false);
        if (interactIcon != null) interactIcon.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (isPlayerNearby && shopPanel != null)
        {
            OpenShop();
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        if (patrol != null) patrol.StartInteraction();
        if (interactIcon != null) interactIcon.SetActive(false);

        // Зупиняємо гравця або викликаємо подію відкриття, якщо потрібно
        Debug.Log("Відкрито магазин: " + shopPanel.name);
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (patrol != null) patrol.StopInteraction();
        if (isPlayerNearby && interactIcon != null) interactIcon.SetActive(true);
    }

    // Логіка тригерів залишається такою ж...
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactIcon != null && (shopPanel == null || !shopPanel.activeSelf))
                interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactIcon != null) interactIcon.SetActive(false);
            CloseShop();
        }
    }
}