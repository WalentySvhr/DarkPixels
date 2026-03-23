using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance; // Синглтон для швидкого доступу

    [Header("Prefabs")]
    public GameObject healTextPrefab; // Сюди перетягни префаб

    [Header("Settings")]
    public Canvas uiCanvas; // Твій основний Canvas
    public Transform playerTransform; // Твій Player (щоб знати позицію)
    public Vector2 textOffset = new Vector2(0f, 1.5f); // Зміщення над головою

    void Awake()
    {
        // Налаштування синглтона
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Головний метод для створення тексту лікування
    public void SpawnHealText(int amount)
    {
        if (healTextPrefab == null || uiCanvas == null || playerTransform == null) return;

        // 1. Беремо позицію гравця у СВІТІ і додаємо зміщення
        Vector3 worldSpawnPos = playerTransform.position + (Vector3)textOffset;

        // 2. ПЕРЕВОДИМО ЇЇ В КООРДИНАТИ ЕКРАНА (пікселі)
        Vector2 screenSpawnPos = Camera.main.WorldToScreenPoint(worldSpawnPos);
        float randomX = Random.Range(-50f, 50f); // Розліт у пікселях
        screenSpawnPos.x += randomX;
        // 3. Створюємо текст
        GameObject newText = Instantiate(healTextPrefab, uiCanvas.transform);
        newText.transform.SetAsLastSibling(); // Поверх усіх

        // 4. Встановлюємо ЕКРАННУ позицію
        newText.transform.position = screenSpawnPos;

        FloatingText ftScript = newText.GetComponent<FloatingText>();
        if (ftScript != null)
        {
            // Передаємо тільки текст, позицію ми вже встановили
            ftScript.SetText("+" + amount, screenSpawnPos);
        }
    }
}