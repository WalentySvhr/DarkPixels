using UnityEngine;

public class FisherNPC : MonoBehaviour
{
    [Header("UI посилання")]
    public GameObject shopFishPanel; // Перетягни сюди панель магазину
    public GameObject interactIcon;  // Іконка рибки над головою

    private bool isPlayerNearby = false;

    void Start()
    {
        // При старті все ховаємо
        if (shopFishPanel != null) shopFishPanel.SetActive(false);
        if (interactIcon != null) interactIcon.SetActive(false);
    }

    // Цей метод працює і для Кліку мишкою (ПК), і для Тапу (Android)
    private void OnMouseDown()
    {
        // Відкриваємо лише якщо гравець підійшов достатньо близько
        if (isPlayerNearby)
        {
            ToggleShop();
        }
        else
        {
            Debug.Log("Гравець занадто далеко, щоб торгувати!");
        }
    }

    public void ToggleShop()
    {
        if (shopFishPanel == null) return;

        bool isOpened = !shopFishPanel.activeSelf;
        shopFishPanel.SetActive(isOpened);

        // Якщо відкрили — можна вимкнути іконку над головою, щоб не заважала
        if (isOpened && interactIcon != null) interactIcon.SetActive(false);
    }

    // Визначаємо, чи поруч гравець (має бути Collider2D з Is Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactIcon != null) interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactIcon != null) interactIcon.SetActive(false);

            // Автоматично закриваємо магазин, якщо гравець просто втік
            if (shopFishPanel != null) shopFishPanel.SetActive(false);
        }
    }
}