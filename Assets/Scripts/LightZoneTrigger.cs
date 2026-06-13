using UnityEngine;

public class LightingZoneTrigger : MonoBehaviour
{
    [Header("Player Sorting Layers")]
    [SerializeField] private string playerOutsideLayer = "Player_Outside";
    [SerializeField] private string playerInsideLayer = "Player_Inside";

    [Header("Enemy Sorting Layers")]
    [SerializeField] private string enemyOutsideLayer = "Enemy_Outside";
    [SerializeField] private string enemyInsideLayer = "Enemy_Inside";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Перевіряємо гравця
        if (other.CompareTag("Player"))
        {
            Debug.Log("Це гравець! Міняємо шар на: " + playerInsideLayer);
            ChangeSortingLayer(other.gameObject, playerInsideLayer);
        }
        // Перевіряємо ворога (переконайся, що у ворогів стоїть тег "Enemy")
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Це ворог! Міняємо шар на: " + enemyInsideLayer);
            ChangeSortingLayer(other.gameObject, enemyInsideLayer);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        // Якщо моб з'явився (респавнився) всередині, але його шар ще "Outside" — примусово міняємо на "Inside"
        if (other.CompareTag("Enemy"))
        {
            SpriteRenderer sr = other.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sortingLayerName == enemyOutsideLayer)
            {
                Debug.Log("Моб заспавнився або знаходиться всередині з неправильним шаром! Виправляємо на: " + enemyInsideLayer);
                ChangeSortingLayer(other.gameObject, enemyInsideLayer);
            }
        }
        else if (other.CompareTag("Player"))
        {
            SpriteRenderer sr = other.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sortingLayerName == playerOutsideLayer)
            {
                ChangeSortingLayer(other.gameObject, playerInsideLayer);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Коли виходить гравець
        if (other.CompareTag("Player"))
        {
            Debug.Log("Гравець вийшов! Міняємо шар на: " + playerOutsideLayer);
            ChangeSortingLayer(other.gameObject, playerOutsideLayer);
        }
        // Коли виходить ворог
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Ворог вийшов! Міняємо шар на: " + enemyOutsideLayer);
            ChangeSortingLayer(other.gameObject, enemyOutsideLayer);
        }
    }

    // Універсальний метод: тепер приймає будь-який об'єкт (гравця чи ворога)
    // Універсальний метод: тепер приймає будь-який об'єкт (гравця чи ворога)
    private void ChangeSortingLayer(GameObject obj, string targetLayer)
    {
        // 1. Міняємо шари для всіх спрайтів (тіло, зброя)
        SpriteRenderer[] allSprites = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in allSprites)
        {
            sprite.sortingLayerName = targetLayer;
        }

        // 2. НОВЕ: Міняємо шари для всіх Canvas (HP бари, текст над головою)
        Canvas[] allCanvases = obj.GetComponentsInChildren<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            canvas.sortingLayerName = targetLayer;
        }
    }
}