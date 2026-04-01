using UnityEngine;

public class FisherNPC : MonoBehaviour
{
    [Header("UI посилання")]
    public GameObject shopFishPanel;
    public GameObject interactIcon;

    private bool isPlayerNearby = false;
    private NPCPatrol patrol; // Посилання на скрипт ходьби

    void Start()
    {
        patrol = GetComponent<NPCPatrol>();
        if (shopFishPanel != null) shopFishPanel.SetActive(false);
        if (interactIcon != null) interactIcon.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (isPlayerNearby)
        {
            OpenShop(); // Змінюємо ToggleShop на OpenShop
        }
    }

    public void OpenShop()
    {
        if (shopFishPanel != null)
        {
            shopFishPanel.SetActive(true);
            if (patrol != null) patrol.StartInteraction();
            if (interactIcon != null) interactIcon.SetActive(false);
            Debug.Log("Магазин має відкритися зараз!");
        }
        else
        {
            Debug.LogError("Посилання на shopFishPanel порожнє!");
        }
    }

    // Цей метод тепер викликається ТІЛЬКИ кнопкою "Закрити" у самому магазині
    public void CloseShop()
    {
        if (shopFishPanel != null) shopFishPanel.SetActive(false);

        // Дозволяємо НПС знову ходити
        if (patrol != null) patrol.StopInteraction();

        // Повертаємо іконку, якщо гравець ще поруч
        if (isPlayerNearby && interactIcon != null) interactIcon.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactIcon != null && !shopFishPanel.activeSelf)
                interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactIcon != null) interactIcon.SetActive(false);
            CloseShop(); // Використовуємо наш новий метод закриття
        }
    }
}